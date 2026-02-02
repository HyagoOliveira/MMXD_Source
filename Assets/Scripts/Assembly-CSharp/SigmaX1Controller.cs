using System;
using UnityEngine;

public class SigmaX1Controller : CharacterControlBase
{
	private enum SkillAnimationID
	{
		ANI_SKILL0_SLASH1_START = 65,
		ANI_SKILL0_SLASH1_LOOP = 66,
		ANI_SKILL0_SLASH1_END = 67,
		ANI_SKILL0_SLASH2_START = 68,
		ANI_SKILL0_SLASH2_LOOP = 69,
		ANI_SKILL0_SLASH2_END = 70,
		ANI_SKILL0_SLASH1_AIR_START = 71,
		ANI_SKILL0_SLASH1_AIR_LOOP = 72,
		ANI_SKILL0_SLASH1_AIR_END = 73,
		ANI_SKILL0_SLASH2_AIR_START = 74,
		ANI_SKILL0_SLASH2_AIR_LOOP = 75,
		ANI_SKILL0_SLASH2_AIR_END = 76,
		ANI_SKILL1_GUARD_START = 77,
		ANI_SKILL1_GUARD_LOOP = 78,
		ANI_SKILL1_GUARD_END = 79,
		ANI_SKILL1_GUARD_HIT = 80,
		ANI_SKILL0_DASH_START = 116,
		ANI_SKILL0_DASH_END = 117,
		ANI_SKILL0_DASH_AIR_START = 118,
		ANI_SKILL0_DASH_AIR_END = 119,
		ANI_SKILL1_START = 143
	}

	private int _dashSlashIndex;

	private Vector3 _dashBeginPosition;

	private Vector3 _dashTargetPosition;

	private Transform _saberFx;

	private IAimTarget _dashTarget;

	private Transform _hurtHeadMesh;

	private Transform _normalHeadMesh;

	private CharacterMaterial _capeMaterial;

	private bool _headCannonGuard;

	private ParticleSystem m_fxuse_skill;

	public float MaxSlashDistance = 5f;

	public float TriggerSlashDistance = 1.5f;

	public int MaxSlashMillisecond = 500;

	private bool _setSlashSpeed;

	private const int SigmaX1BuffID = -1;

	private bool _capeDisappearFlag;

	private bool _capeAppeaFlag;

	private bool _headCannonActive
	{
		get
		{
			return _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1);
		}
		set
		{
			if (value)
			{
				_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0, !_refEntity.IsLocalPlayer);
			}
			else
			{
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(-1);
			}
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[16]
		{
			"ch038_skill_01_stand_1st_atk_start", "ch038_skill_01_stand_1st_atk_loop", "ch038_skill_01_stand_1st_atk_end", "ch038_skill_01_stand_2nd_atk_start", "ch038_skill_01_stand_2nd_atk_loop", "ch038_skill_01_stand_2nd_atk_end", "ch038_skill_01_jump_1st_atk_start", "ch038_skill_01_jump_1st_atk_loop", "ch038_skill_01_jump_1st_atk_end", "ch038_skill_01_jump_2nd_atk_start",
			"ch038_skill_01_jump_2nd_atk_loop", "ch038_skill_01_jump_2nd_atk_end", "ch038_skill_guard_hited", "ch038_skill_guard_loop", "ch038_skill_guard_end", "ch038_skill_guard_hited"
		};
	}

	public override string[] GetCharacterDependBlendAnimations()
	{
		return new string[4] { "ch038_skill_01_stand_1st_start", "ch038_skill_01_stand_1st_loop", "ch038_skill_01_jump_1st_start", "ch038_skill_01_jump_1st_loop" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[6] { "stand_special0", "stand_special1", "stand_special2", "normal_damage_start", "normal_damage_loop", "normal_damage_end" };
		target = new string[6] { "ch038_skill_02_stand_up", "ch038_skill_02_stand_mid", "ch038_skill_02_stand_down", "ch038_hurt_start", "ch038_hurt_loop", "ch038_hurt_end" };
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			_refEntity.IgnoreGravity = false;
			_refEntity.BulletCollider.BackToPool();
			break;
		}
		_refEntity.Dashing = false;
		_refEntity.EnableCurrentWeapon();
		MyToggleExtraMesh(false);
		_headCannonActive = false;
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void Start()
	{
		base.Start();
		_headCannonGuard = _refEntity.PlayerSkills[1].BulletData.s_USE_MOTION == "SIGMA_GUARD";
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_saberFx = OrangeBattleUtility.FindChildRecursive(ref target, "SaberFX_G", true);
		_capeMaterial = OrangeBattleUtility.FindChildRecursive(ref target, "CapeController", true).GetComponent<CharacterMaterial>();
		_normalHeadMesh = OrangeBattleUtility.FindChildRecursive(ref target, "HeadMeshStand_c", true);
		_hurtHeadMesh = OrangeBattleUtility.FindChildRecursive(ref target, "HeadMeshHurt_c", true);
		_refEntity.ExtraTransforms = new Transform[1];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Head", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_skill", true);
		if (transform != null)
		{
			m_fxuse_skill = transform.GetComponent<ParticleSystem>();
		}
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, "SaberMesh_m");
		if (array != null)
		{
			OrangeCharacter refEntity = _refEntity;
			Renderer[] extraMeshOpen = new SkinnedMeshRenderer[array.Length];
			refEntity.ExtraMeshOpen = extraMeshOpen;
			for (int i = 0; i < array.Length; i++)
			{
				_refEntity.ExtraMeshOpen[i] = array[i].GetComponent<SkinnedMeshRenderer>();
			}
		}
		array = OrangeBattleUtility.FindAllChildRecursive(ref target, "HandleMesh_m");
		if (array != null)
		{
			OrangeCharacter refEntity2 = _refEntity;
			Renderer[] extraMeshOpen = new SkinnedMeshRenderer[array.Length];
			refEntity2.ExtraMeshClose = extraMeshOpen;
			for (int j = 0; j < array.Length; j++)
			{
				_refEntity.ExtraMeshClose[j] = array[j].GetComponent<SkinnedMeshRenderer>();
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("p_Sigma_skill1_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("p_Sigma_skill1_000_L", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("p_Sigma_skill1_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("p_Sigma_skill1_001_L", 2);
		MyToggleExtraMesh(false);
		ToggleHurtHeadMesh(false);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.GetUpperAnimateKeepFlagEvt = GetUpperAnimateKeepFlag;
		_refEntity.GuardCalculateEvt = GuardCalculate;
		_refEntity.PlayerPressSkillCB = PlayerPressSkill;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
	}

	public void TeleportInCharacterDepend()
	{
		if (!(_refEntity.CurrentFrame > 0.4f) || _capeDisappearFlag)
		{
			return;
		}
		_capeMaterial.Disappear(delegate
		{
			Renderer[] renderer = _capeMaterial.GetRenderer();
			for (int i = 0; i < renderer.Length; i++)
			{
				renderer[i].enabled = false;
			}
		});
		_capeDisappearFlag = true;
	}

	public void TeleportOutCharacterDepend()
	{
		if (!_capeAppeaFlag)
		{
			_refEntity.CharacterMaterials.SetSubCharacterMaterial(_capeMaterial);
			_capeAppeaFlag = true;
			Renderer[] renderer = _capeMaterial.GetRenderer();
			for (int i = 0; i < renderer.Length; i++)
			{
				renderer[i].enabled = true;
			}
			_capeMaterial.Appear();
		}
	}

	public override void CheckSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus != OrangeCharacter.SubStatus.SKILL0)
			{
				break;
			}
			if (!_setSlashSpeed)
			{
				if (_refEntity.CurrentFrame > 0.77f)
				{
					_refEntity.PlaySE(_refEntity.SkillSEID, "sg_blade01");
					_setSlashSpeed = true;
					_dashTarget = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
					if (_dashTarget != null)
					{
						_dashTargetPosition = _dashTarget.AimPosition;
						Vector2 vector = (_dashTargetPosition - _refEntity.AimPosition).normalized * ((float)OrangeCharacter.DashSpeed * 4f);
						_refEntity.direction = Math.Sign(vector.x);
						_refEntity.SetSpeed((int)vector.x, (int)vector.y);
					}
					else
					{
						_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 4f), 0);
					}
				}
			}
			else if (Vector2.Distance(_dashBeginPosition, _refEntity.AimPosition) > MaxSlashDistance || (_dashTarget != null && Vector2.Distance(_dashTargetPosition, _refEntity.AimPosition) < TriggerSlashDistance) || _refEntity.PlayerSkills[0].LastUseTimer.GetMillisecond() > MaxSlashMillisecond)
			{
				_refEntity.PlaySE(_refEntity.SkillSEID, "sg_blade02");
				switch (_dashSlashIndex)
				{
				case 0:
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
					break;
				case 1:
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
					break;
				}
			}
			break;
		}
		case 1:
		{
			bool flag = ManagedSingleton<InputStorage>.Instance.IsPressed(_refEntity.UserID, ButtonId.SHOOT);
			if (flag || ManagedSingleton<InputStorage>.Instance.IsPressed(_refEntity.UserID, ButtonId.SKILL0))
			{
				ClearSkill();
				if ((bool)_refEntity.Controller.ObjectMeeting(0f, -0.15f, (int)_refEntity.Controller.collisionMask | (int)_refEntity.Controller.collisionMaskThrough))
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				if (flag)
				{
					_refEntity.PlayerHeldShootCB.CheckTargetToInvoke();
				}
			}
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 49) <= 1u && !_headCannonActive)
			{
				_refEntity.SetStatus(_refEntity.CurMainStatus, OrangeCharacter.SubStatus.SKILL1_2);
			}
			break;
		}
		}
		if (_headCannonActive && _refEntity.PlayerSkills[1].MagazineRemain > 0f)
		{
			if (_refEntity.PlayerSkills[1].LastUseTimer.IsStarted())
			{
				if (_refEntity.PlayerSkills[1].LastUseTimer.GetMillisecond() <= _refEntity.PlayerSkills[1].BulletData.n_FIRE_SPEED)
				{
					return;
				}
			}
			else
			{
				_refEntity.PlayerSkills[1].LastUseTimer.TimerStart();
			}
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
			_refEntity.PlayerAutoAimSystem.UpdateAimRange(_refEntity.PlayerSkills[1].BulletData.f_DISTANCE);
			_refEntity.UpdateAimDirection();
			CreateSkillBullet(_refEntity.PlayerSkills[1]);
			_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[0]);
			_refEntity.PlaySE(_refEntity.SkillSEID, "sg_magnum01");
		}
		else
		{
			_headCannonActive = false;
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.IDLE && _refEntity.IsShoot == 0)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.SKILL_IDLE);
		}
		_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV);
	}

	public override void ExtraVariableInit()
	{
		if (m_fxuse_skill != null && m_fxuse_skill.isPlaying)
		{
			m_fxuse_skill.Stop();
		}
	}

	public void PlayerPressSkill(int id)
	{
		if ((int)_refEntity.Hp <= 0 || id == 0 || id != 1 || _refEntity.CurrentActiveSkill == 0 || _refEntity.PlayerSkills[id].MagazineRemain <= 0f || _refEntity.PlayerSkills[id].ForceLock)
		{
			return;
		}
		if (!_refEntity.CheckUseSkillKeyTrigger(id))
		{
			if (_headCannonActive)
			{
				_headCannonActive = false;
			}
			return;
		}
		if (_headCannonGuard)
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.ExtraTransforms[0]);
			_refEntity.DisableCurrentWeapon();
			MyToggleExtraMesh(true);
			_refEntity.SetSpeed(0, 0);
		}
		_headCannonActive = !_headCannonActive;
		if (_headCannonActive)
		{
			_refEntity.PlaySE(_refEntity.VoiceID, "v_sg_skill02");
		}
		if (_headCannonGuard && _refEntity.PlayerSkills[1].LastUseTimer.IsStarted())
		{
			_refEntity.PlayerSkills[1].LastUseTimer.TimerStop();
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp > 0 && id == 0 && _refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			_dashBeginPosition = _refEntity.AimPosition;
			_setSlashSpeed = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.ExtraTransforms[0]);
			_refEntity.DisableCurrentWeapon();
			MyToggleExtraMesh(true);
			_refEntity.IgnoreGravity = true;
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerSkills[0].LastUseTimer.TimerStart();
			_refEntity.PlaySE(_refEntity.VoiceID, "v_sg_skill01");
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		bool flag = _refEntity.Controller.ObjectMeeting(0f, -0.15f, (int)_refEntity.Controller.collisionMask | (int)_refEntity.Controller.collisionMaskThrough);
		if ((mainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.CurSubStatus != OrangeCharacter.SubStatus.SKILL0) && m_fxuse_skill != null && m_fxuse_skill.isPlaying)
		{
			m_fxuse_skill.Stop();
		}
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_dashSlashIndex = _refEntity.PlayerSkills[0].Reload_index;
				if (m_fxuse_skill != null && !m_fxuse_skill.isPlaying)
				{
					m_fxuse_skill.Play();
				}
				_refEntity.SetAnimateId((HumanBase.AnimateId)(116 + ((!flag) ? 2 : 0)));
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetSpeed(0, 0);
				if (_dashTarget != null)
				{
					Vector2 vector2 = _dashTarget.AimPosition - _refEntity.AimPosition;
					_refEntity.direction = Math.Sign(vector2.x);
				}
				if (_refEntity.ShootDirection.x > 0f)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("p_Sigma_skill1_000", _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				}
				else
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("p_Sigma_skill1_000_L", _refEntity.ModelTransform.position, OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				}
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[0].FastBulletDatas[0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[0].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[0].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.SetAnimateId((HumanBase.AnimateId)(65 + ((!flag) ? 6 : 0)));
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.SetSpeed(0, 0);
				if (_dashTarget != null)
				{
					Vector2 vector = _dashTarget.AimPosition - _refEntity.AimPosition;
					_refEntity.direction = Math.Sign(vector.x);
				}
				if (_refEntity.ShootDirection.x > 0f)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("p_Sigma_skill1_001", _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				}
				else
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("p_Sigma_skill1_001_L", _refEntity.ModelTransform.position, OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				}
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[0].FastBulletDatas[1], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[0].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[0].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.PlayerSkills[0].MagazineRemain -= _refEntity.PlayerSkills[0].FastBulletDatas[1].n_USE_COST;
				_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.ExtraTransforms[0]);
				_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[0].ComboCheckDatas[0].nComboSkillID);
				_refEntity.PlayerSkills[0].Reload_index = 0;
				_refEntity.SetAnimateId((HumanBase.AnimateId)(68 + ((!flag) ? 6 : 0)));
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)77u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)78u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				MyToggleExtraMesh(false);
				_refEntity.SetAnimateId((HumanBase.AnimateId)79u);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.IDLE:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.PALETTE_ATTACK_GROUND:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.PALETTE_ATTACK_AIR:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL_IDLE:
				break;
			}
			break;
		case OrangeCharacter.MainStatus.FALL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.LAND:
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				break;
			case OrangeCharacter.SubStatus.DASH_END:
				_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
				break;
			}
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (_refEntity.LastSetAnimateFrame > _refEntity.Animator.LastUpdateFrame)
		{
			return;
		}
		bool flag = _refEntity.Controller.ObjectMeeting(0f, -0.15f, (int)_refEntity.Controller.collisionMask | (int)_refEntity.Controller.collisionMaskThrough);
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.IDLE:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL_IDLE:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, _headCannonActive ? OrangeCharacter.SubStatus.SKILL_IDLE : OrangeCharacter.SubStatus.IDLE);
				break;
			case OrangeCharacter.SubStatus.PALETTE_ATTACK_GROUND:
			case OrangeCharacter.SubStatus.PALETTE_ATTACK_AIR:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.FALL:
			if ((uint)(subStatus - 3) <= 2u)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.CurrentActiveSkill = -1;
				_refEntity.IgnoreGravity = false;
				MyToggleExtraMesh(false);
				_refEntity.EnableCurrentWeapon();
				_refEntity.Dashing = false;
				_refEntity.BulletCollider.BackToPool();
				if (flag)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.PALETTE_ATTACK_GROUND);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.LAND);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.CurrentActiveSkill = -1;
				_refEntity.IgnoreGravity = false;
				MyToggleExtraMesh(false);
				_refEntity.EnableCurrentWeapon();
				_refEntity.Dashing = false;
				_refEntity.BulletCollider.BackToPool();
				if (flag)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.PALETTE_ATTACK_AIR);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.DASH_END);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetStatus(_refEntity.CurMainStatus, OrangeCharacter.SubStatus.SKILL1_1);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.CurrentActiveSkill = -1;
				_refEntity.EnableCurrentWeapon();
				if (flag)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				break;
			}
			break;
		}
	}

	public bool GetUpperAnimateKeepFlag(HumanBase.AnimateId id)
	{
		if (_refEntity.IsAnimateIDChanged())
		{
			switch (_refEntity.AnimateID)
			{
			case HumanBase.AnimateId.ANI_SLASH1:
				if (_refEntity.AnimateIDPrev == HumanBase.AnimateId.ANI_WALKSLASH1)
				{
					return true;
				}
				return false;
			case HumanBase.AnimateId.ANI_WALKSLASH1:
				if (_refEntity.AnimateIDPrev == HumanBase.AnimateId.ANI_SLASH1)
				{
					return true;
				}
				return false;
			case HumanBase.AnimateId.ANI_SLASH2:
				if (_refEntity.AnimateIDPrev == HumanBase.AnimateId.ANI_WALKSLASH2)
				{
					return true;
				}
				return false;
			case HumanBase.AnimateId.ANI_WALKSLASH2:
				if (_refEntity.AnimateIDPrev == HumanBase.AnimateId.ANI_SLASH2)
				{
					return true;
				}
				return false;
			default:
				return false;
			}
		}
		if (_refEntity.IsShoot != _refEntity.IsShootPrev && id == HumanBase.AnimateId.ANI_STAND)
		{
			return false;
		}
		if (_refEntity.IsShoot == 0 && !_headCannonActive)
		{
			return true;
		}
		if ((id == HumanBase.AnimateId.ANI_STAND || id == HumanBase.AnimateId.ANI_CROUCH_END || id == HumanBase.AnimateId.ANI_STAND_SKILL) && _refEntity.IsShoot < 3 && _refEntity.FreshBullet)
		{
			return false;
		}
		return true;
	}

	protected void MyToggleExtraMesh(bool open)
	{
		_refEntity.ToggleExtraMesh(open);
		_saberFx.gameObject.SetActive(open);
	}

	public override bool GuardCalculate(HurtPassParam tHurtPassParam)
	{
		if ((int)_refEntity.Hp > 0)
		{
			if (_headCannonGuard)
			{
				return _headCannonActive;
			}
			return false;
		}
		return false;
	}

	public void ToggleHurtHeadMesh(bool isHurt)
	{
		_hurtHeadMesh.gameObject.SetActive(isHurt);
		_normalHeadMesh.gameObject.SetActive(!isHurt);
	}

	public override void SetStun(bool enable)
	{
		ToggleHurtHeadMesh(enable);
	}
}
