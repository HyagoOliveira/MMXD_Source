using System;
using System.Collections.Generic;
using Better;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class CH119_Controller : CharacterControllerProxyBaseGen4
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL0_STEP2_START = 65u,
		ANI_SKILL0_STEP2_LOOP = 66u,
		ANI_SKILL0_STEP2_END = 67u,
		ANI_SKILL1_START = 68u,
		ANI_SKILL1_LOOP = 69u,
		ANI_SKILL1_END_STAND = 70u,
		ANI_SKILL1_END_JUMP = 71u,
		ANI_SKILL0_STEP1_STAND = 127u,
		ANI_SKILL0_STEP1_CROUCH = 128u,
		ANI_SKILL0_STEP1_JUMP = 129u
	}

	private enum FxName
	{
		fxuse_drhadoken_000 = 0,
		fxuse_drpunch_000 = 1,
		fxuse_drpunch_002_2 = 2
	}

	public int SKILL_0_LOOP_COUNT = 3;

	public float SKILL_0_LOOP_ANIMATION_SPEED = 2f;

	public float TIME_SKILL_0_0_CANCEL = 0.15f;

	public float TIME_SKILL_0_1_CANCEL = 0.05f;

	public float TIME_SKILL_0_HITPAUSE = 0.1f;

	public float SKILL_1_RUSH_SPEED = 4f;

	public float SKILL_1_MOVE_SPEED = 0.1f;

	public int SKILL_1_LOOP_COUNT = 8;

	public int SKILL_1_LOOP_COUNT_CANCEL = 5;

	public float SKILL_1_LOOP_ANIMATION_SPEED = 8f;

	public float TIME_SKILL_1_CANCEL = 0.05f;

	private OrangeConsoleCharacter _refConsolePlayer;

	private EventManager.StageCameraFocus stageCameraFocus;

	private int _skillLoopRemain;

	private ParticleSystem _skill0TornadoFx;

	private SKILL_TABLE _skill0ComboSkillData;

	private SKILL_TABLE _skill1LinkSkillData;

	private IAimTarget _skill1RushTarget;

	private Transform _skill1HitTransform;

	private Vector3 _skill1RushStartPos;

	private Vector2 _skill1RushVelocity;

	private RushCollideBullet _skill1RushCollideBullet;

	private bool _isSyncRushSkillCompleted;

	private FxBase _skill1ShungokusatsuFx;

	private CH100_ShungokusatsuBullet _skill1ShungokusatsuBullet;

	private ParticleSystem _skill1RushFx;

	private ParticleSystem _skill1PunchFx;

	private void InitLinkSkill()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
		if (_skill1LinkSkillData == null && weaponStruct.BulletData.n_LINK_SKILL != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out _skill1LinkSkillData))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref _skill1LinkSkillData);
		}
	}

	private void OnBulletColliderHit(object obj)
	{
		StartHitPause(TIME_SKILL_0_HITPAUSE);
	}

	private void OnHitPauseChangedEvent(bool isStarted)
	{
		if (isStarted)
		{
			ParticleSystem skill0TornadoFx = _skill0TornadoFx;
			if ((object)skill0TornadoFx != null)
			{
				skill0TornadoFx.Pause(true);
			}
		}
		else
		{
			ParticleSystem skill0TornadoFx2 = _skill0TornadoFx;
			if ((object)skill0TornadoFx2 != null)
			{
				skill0TornadoFx2.Play(true);
			}
		}
	}

	private void ForceUpdateVirtualButtonAnalog()
	{
		if (_refEntity.IsLocalPlayer)
		{
			OrangeConsoleCharacter refConsolePlayer = _refConsolePlayer;
			if ((object)refConsolePlayer != null)
			{
				refConsolePlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, true);
			}
		}
	}

	private void CreateSkill1PunchBullet(WeaponStruct wsSkill)
	{
		if (_skill1ShungokusatsuBullet != null && _skill1RushTarget != null)
		{
			if (_refEntity.IsLocalPlayer && _skill1HitTransform != null)
			{
				_skill1ShungokusatsuBullet.SetHitTarget(_skill1HitTransform);
			}
			else
			{
				StageObjBase stageObjBase = _skill1RushTarget as StageObjBase;
				if (stageObjBase != null)
				{
					_skill1ShungokusatsuBullet.SetHitTarget(stageObjBase._transform);
				}
				else
				{
					_skill1ShungokusatsuBullet.SetHitTarget(_skill1RushTarget.AimTransform);
				}
			}
			_skill1ShungokusatsuBullet.UpdateBulletData(_skill1LinkSkillData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
			_skill1ShungokusatsuBullet.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_skill1ShungokusatsuBullet.BulletLevel = wsSkill.SkillLV;
			_skill1ShungokusatsuBullet.Active(_skill1RushTarget.AimPosition, _refEntity.ShootDirection, _refEntity.TargetMask);
		}
		_skill1RushTarget = null;
	}

	private void SetSkill1RushVelocity(Vector3 rushVector)
	{
		_skill1RushVelocity = rushVector.normalized * OrangeCharacter.DashSpeed * SKILL_1_RUSH_SPEED;
		_isSyncRushSkillCompleted = true;
	}

	private void UpdateSkill1Direction(Vector2 rushVector)
	{
		int num = (int)Mathf.Sign(rushVector.x);
		if (num != 0 && _refEntity.direction != num)
		{
			_refEntity.direction = num;
		}
	}

	private void UpdateRushFxRotation(Vector2 rushVector)
	{
		float num = Mathf.Atan2(rushVector.y, rushVector.x);
		Vector3 eulerAngles = _skill1RushFx.transform.eulerAngles;
		eulerAngles.z = ((_refEntity.direction > 0) ? (num * 57.29578f) : (((float)Math.PI - num) * 57.29578f));
		_skill1RushFx.transform.eulerAngles = eulerAngles;
	}

	private void RecycleRushCollideBullet()
	{
		if ((bool)_skill1RushCollideBullet)
		{
			_skill1RushCollideBullet.BackToPool();
			_skill1RushCollideBullet.HitCallback = null;
			_skill1RushCollideBullet = null;
		}
	}

	private void Skill1CollideHitCB(object obj)
	{
		if (!_refEntity.IsLocalPlayer && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			return;
		}
		if (_refEntity.UsingVehicle)
		{
			RecycleRushCollideBullet();
			return;
		}
		Collider2D collider2D = obj as Collider2D;
		if (collider2D == null)
		{
			return;
		}
		Transform transform = collider2D.transform;
		StageObjParam stageObjParam = transform.GetComponent<StageObjParam>();
		if (stageObjParam == null)
		{
			PlayerCollider component = transform.GetComponent<PlayerCollider>();
			if (component != null && component.IsDmgReduceShield())
			{
				stageObjParam = component.GetDmgReduceOwner();
			}
		}
		if (stageObjParam == null || stageObjParam.tLinkSOB == null)
		{
			return;
		}
		OrangeCharacter orangeCharacter = stageObjParam.tLinkSOB as OrangeCharacter;
		EnemyControllerBase enemyControllerBase = stageObjParam.tLinkSOB as EnemyControllerBase;
		if (!orangeCharacter && !enemyControllerBase)
		{
			return;
		}
		RecycleRushCollideBullet();
		if (_refEntity.IsLocalPlayer)
		{
			if ((bool)orangeCharacter)
			{
				_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0, false, orangeCharacter.sPlayerID);
				_skill1RushTarget = orangeCharacter;
			}
			else
			{
				_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0);
				_skill1RushTarget = enemyControllerBase;
			}
			_skill1HitTransform = transform;
		}
	}

	private void ActionStatusChanged_0_0()
	{
		ResetSpeed();
		SetIgnoreGravity();
		if (_refEntity.IsInGround)
		{
			if (_refEntity.IsCrouching)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
			}
			else
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
			}
		}
		else
		{
			_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
		}
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.IsShoot = 3;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, _refEntity.ShootDirection);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		Vector3 shootDirection = _refEntity.ShootDirection;
		float num = Mathf.Atan2(shootDirection.y, shootDirection.x);
		Vector3 one = Vector3.one;
		if (Mathf.Abs(num) - 90f > 0f)
		{
			num = (180f - Mathf.Abs(num)) * Mathf.Sign(num);
			one.x = -1f;
		}
		PlayVoiceSE("v_dr_skill01_1");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_drhadoken_000.ToString(), _refEntity.ExtraTransforms[0].position, Quaternion.Euler(new Vector3(0f, 0f, num * 57.29578f)), new object[1] { one });
		SetSkillCancelFrame(TIME_SKILL_0_0_CANCEL);
	}

    [Obsolete]
    private void ActionStatusChanged_0_10()
	{
		ResetSpeed();
		SetIgnoreGravity();
		CollideBullet bulletCollider = _refEntity.BulletCollider;
		bulletCollider.HitCallback = (CallbackObj)Delegate.Combine(bulletCollider.HitCallback, new CallbackObj(OnBulletColliderHit));
		base.OnHitPauseStateChangedEvent += OnHitPauseChangedEvent;
		PerBuff perBuff;
		int num = StageUpdate.runPlayers.FindLastIndex((OrangeCharacter player) => !player.IsDead() && player.selfBuffManager.CheckHasMarkedEffect(115, _refEntity.sNetSerialID, out perBuff) && perBuff.sPlayerID == _refEntity.sPlayerID);
		if (num >= 0)
		{
			OrangeCharacter orangeCharacter = StageUpdate.runPlayers[num];
			_refEntity.Controller.LogicPosition = new VInt3(orangeCharacter._transform.position);
			_refEntity._transform.position = orangeCharacter._transform.position;
			if (_refEntity.IsLocalPlayer)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
			}
		}
		ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_skill0ComboSkillData = weaponStruct.FastBulletDatas[weaponStruct.Reload_index];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, _skill0ComboSkillData, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		_refEntity.RemoveComboSkillBuff(_skill0ComboSkillData.n_ID);
		_skillLoopRemain = SKILL_0_LOOP_COUNT;
		PlayVoiceSE("v_dr_skill01_2");
		ParticleSystem skill0TornadoFx = _skill0TornadoFx;
		if ((object)skill0TornadoFx != null)
		{
			skill0TornadoFx.Play(true);
		}
		PlaySkillSE("dr_hadou03");
		SetSkillFrame(0.01f);
	}

	private void ActionStatusChanged_0_11()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.BulletCollider.UpdateBulletData(_skill0ComboSkillData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
		_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
		_refEntity.BulletCollider.Active(_refEntity.TargetMask);
		_refEntity.ForceSetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
		_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
		_refEntity.Animator._animator.speed = SKILL_0_LOOP_ANIMATION_SPEED;
	}

	private void ActionAnimationEnd_0_11()
	{
		_refEntity.BulletCollider.BackToPool();
		_skillLoopRemain--;
		if (_skillLoopRemain > 0)
		{
			SetSkillStatus(base.CurSubStatus);
		}
		else
		{
			ShiftSkillStatus();
		}
	}

	private void ActionStatusChanged_0_12()
	{
		_refEntity.Animator._animator.speed = 1f;
		_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
		SetSkillCancelFrame(TIME_SKILL_0_1_CANCEL);
	}

	private void ActionStatusChanged_1_0()
	{
		ResetSpeed();
		SetIgnoreGravity(false);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		_isSyncRushSkillCompleted = false;
		if (_refEntity.IsLocalPlayer)
		{
			IAimTarget autoAimTarget = base.AimSystem.AutoAimTarget;
			if (_refEntity.UseAutoAim && autoAimTarget != null)
			{
				_skill1RushTarget = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
				SetSkill1RushVelocity(autoAimTarget.AimPosition - _refEntity.AimPosition);
			}
			else
			{
				_skill1RushTarget = null;
				SetSkill1RushVelocity(_refEntity.ShootDirection);
			}
		}
		_skill1RushStartPos = _refEntity.AimPosition;
		_skillLoopRemain = SKILL_1_LOOP_COUNT;
		PlayVoiceSE("v_dr_skill02");
	}

	private void ActionLogicUpdate_1_0()
	{
		if (_isSyncRushSkillCompleted)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
			_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.AimTransform, weaponStruct.SkillLV, _skill1RushVelocity, true, null, Skill1CollideHitCB);
			UpdateSkill1Direction(_skill1RushVelocity);
			if ((bool)_skill1RushFx)
			{
				UpdateRushFxRotation(_skill1RushVelocity);
				_skill1RushFx.Play(true);
			}
			float num = Mathf.Atan2(_skill1RushVelocity.y, _skill1RushVelocity.x);
			Vector3 one = Vector3.one;
			if (Mathf.Abs(num) > (float)Math.PI / 2f)
			{
				num = ((float)Math.PI - Mathf.Abs(num)) * Mathf.Sign(num);
				one.x = -1f;
			}
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_drpunch_000.ToString(), _refEntity.ModelTransform, Quaternion.Euler(new Vector3((0f - num) * 57.29578f, 0f, 0f)), one, Array.Empty<object>());
			ShiftSkillStatus();
		}
	}

	private void ActionStatusChanged_1_1()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.SetSpeed((int)_skill1RushVelocity.x, (int)_skill1RushVelocity.y);
		_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
	}

	private void ActionLogicUpdate_1_1()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		bool flag = false;
		if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			flag = true;
			Vector3 p_worldPos = _refEntity.AimPosition + new Vector3(1f, 0f, 0f) * _refEntity.direction;
			if (_refEntity.IsLocalPlayer && _skill1HitTransform != null)
			{
				p_worldPos = _skill1HitTransform.position;
			}
			else if (_skill1RushTarget != null)
			{
				p_worldPos = _skill1RushTarget.AimPosition;
			}
			_skill1ShungokusatsuFx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FxName.fxuse_drpunch_002_2.ToString(), p_worldPos, Quaternion.identity, Array.Empty<object>());
		}
		else if (Vector2.Distance(_skill1RushStartPos, _refEntity.AimPosition) > weaponStruct.BulletData.f_DISTANCE || weaponStruct.LastUseTimer.GetMillisecond() > 350)
		{
			flag = true;
			_skill1RushTarget = null;
		}
		if (flag)
		{
			ResetSpeed();
			RecycleRushCollideBullet();
			ParticleSystem skill1RushFx = _skill1RushFx;
			if ((object)skill1RushFx != null)
			{
				skill1RushFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			PlaySkillSE("dr_renda02");
			ParticleSystem skill1PunchFx = _skill1PunchFx;
			if ((object)skill1PunchFx != null)
			{
				skill1PunchFx.Play(true);
			}
			if (_skill1RushTarget != null)
			{
				CreateSkill1PunchBullet(weaponStruct);
			}
			ShiftSkillStatus();
		}
	}

	private void ActionStatusChanged_1_2()
	{
		Vector2 vector = _skill1RushVelocity.normalized * OrangeCharacter.WalkSpeed * SKILL_1_MOVE_SPEED;
		SetSpeed(vector.x, vector.y);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.ForceSetAnimateId((HumanBase.AnimateId)68u);
		_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
		_refEntity.Animator._animator.speed = SKILL_1_LOOP_ANIMATION_SPEED;
	}

	private void ActionAnimationEnd_1_2()
	{
		_refEntity.BulletCollider.BackToPool();
		_skillLoopRemain--;
		if (_skillLoopRemain > 0)
		{
			SetSkillStatus(base.CurSubStatus);
			if (_skill1ShungokusatsuBullet.bIsEnd && _skillLoopRemain <= SKILL_1_LOOP_COUNT_CANCEL)
			{
				ActionCheckSkillCancel();
			}
		}
		else
		{
			ShiftSkillStatus();
		}
	}

	private void ActionStatusChanged_1_3()
	{
		ResetSpeed();
		ParticleSystem skill1PunchFx = _skill1PunchFx;
		if ((object)skill1PunchFx != null)
		{
			skill1PunchFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		_refEntity.Animator._animator.speed = 1f;
		if ((bool)_refEntity.Controller.BelowInBypassRange)
		{
			_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
		}
		else
		{
			_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
		}
		SetSkillCancelFrame(TIME_SKILL_1_CANCEL);
	}

	public override void Awake()
	{
		base.Awake();
		if (_refEntity is OrangeConsoleCharacter)
		{
			_refConsolePlayer = _refEntity as OrangeConsoleCharacter;
		}
		_refEntity.ExtraTransforms = new Transform[2]
		{
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "L WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "R WeaponPoint", true)
		};
		_skill0TornadoFx = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "fxuse_drkick_000", true).GetComponent<ParticleSystem>();
		ParticleSystem skill0TornadoFx = _skill0TornadoFx;
		if ((object)skill0TornadoFx != null)
		{
			skill0TornadoFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		_skill1RushFx = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "fxuse_drpunch_001", true).GetComponent<ParticleSystem>();
		ParticleSystem skill1RushFx = _skill1RushFx;
		if ((object)skill1RushFx != null)
		{
			skill1RushFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		_skill1PunchFx = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "fxuse_drpunch_002", true).GetComponent<ParticleSystem>();
		ParticleSystem skill1PunchFx = _skill1PunchFx;
		if ((object)skill1PunchFx != null)
		{
			skill1PunchFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		_skill1ShungokusatsuBullet = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "p_shungokusatsu_000", true).GetComponent<CH100_ShungokusatsuBullet>();
	}

	public override void Start()
	{
		base.Start();
		_refEntity.AnimatorModelShiftYOverride = new Better.Dictionary<OrangeCharacter.MainStatus, float>();
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
		stageCameraFocus = new EventManager.StageCameraFocus
		{
			bLock = true,
			bRightNow = true
		};
		InitLinkSkill();
		InitializeSkillDependDelegators(new System.Collections.Generic.Dictionary<OrangeCharacter.SubStatus, SkillStateDelegateData>
		{
			{
				OrangeCharacter.SubStatus.SKILL0,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_0,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_10,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_10,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_11,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_11,
					OnAnimationEnd = ActionAnimationEnd_0_11
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_12,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_12,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_0,
					OnLogicUpdate = ActionLogicUpdate_1_0
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_1,
					OnLogicUpdate = ActionLogicUpdate_1_1
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_2,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_2,
					OnAnimationEnd = ActionAnimationEnd_1_2
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_3,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_3,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			}
		});
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[7] { "ch119_skill_01_step2_start", "ch119_skill_01_step2_loop", "ch119_skill_01_step2_end", "ch119_skill_02_start", "ch119_skill_02_loop", "ch119_skill_02_end_stand", "ch119_skill_02_end_jump" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		return new string[3][]
		{
			new string[3] { "ch119_skill_01_step1_stand_up", "ch119_skill_01_step1_stand_mid", "ch119_skill_01_step1_stand_down" },
			new string[3] { "ch119_skill_01_step1_crouch_up", "ch119_skill_01_step1_crouch_mid", "ch119_skill_01_step1_crouch_down" },
			new string[3] { "ch119_skill_01_step1_jump_up", "ch119_skill_01_step1_jump_mid", "ch119_skill_01_step1_jump_down" }
		};
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "login", "logout", "win" };
		target = new string[3] { "ch119_login", "ch119_logout", "ch119_win" };
	}

	protected override void TeleportInCharacterDepend()
	{
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID == HumanBase.AnimateId.ANI_TELEPORT_IN_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
		}
	}

	protected override void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
		}
	}

	protected override void StageTeleportInCharacterDepend()
	{
	}

	protected override void StageTeleportOutCharacterDepend()
	{
	}

	public override void ControlCharacterContinue()
	{
	}

	protected override void ToggleExtraTransforms(bool isActive)
	{
	}

	protected override bool OnEnterRideArmor(RideBaseObj targetRideArmor)
	{
		return base.OnEnterRideArmor(targetRideArmor);
	}

	protected override void ToggleWeapon(WeaponState weaponState)
	{
		if ((uint)weaponState <= 1u || (uint)(weaponState - 3) <= 1u)
		{
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
		}
		else
		{
			ToggleNormalWeapon(true);
		}
	}

	protected override void AttachSkillDelegateEvent()
	{
		OnPlayerReleaseSkill0Events[0] = OnPlayerReleaseSkill0_0;
		OnPlayerPressSkill0Events[1] = OnPlayerPressSkill0_1;
		OnPlayerReleaseSkill1Events[0] = OnPlayerReleaseSkill1;
	}

	protected void OnPlayerReleaseSkill0_0(SkillID skillID)
	{
		base.OnPlayerReleaseSkill0(skillID);
	}

	protected void OnPlayerPressSkill0_1(SkillID skillID)
	{
		PlayerStopDashing();
		SetSkillAndWeapon(skillID);
		SetSkillStatus(OrangeCharacter.SubStatus.SKILL0_10);
	}

	protected override void OnPlayerReleaseSkill1(SkillID skillID)
	{
		base.OnPlayerReleaseSkill1(skillID);
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			ForceStopHitPause();
		}
	}

	public override void ControlCharacterDead()
	{
		ForceStopHitPause();
	}

	protected override void OnChangeComboSkill(SkillID skillId, int reloadIndex)
	{
		if (skillId != 0)
		{
			return;
		}
		switch (reloadIndex)
		{
		case 0:
		{
			OrangeConsoleCharacter refConsolePlayer3 = _refConsolePlayer;
			if ((object)refConsolePlayer3 != null)
			{
				refConsolePlayer3.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, true);
			}
			break;
		}
		case 1:
		{
			OrangeConsoleCharacter refConsolePlayer = _refConsolePlayer;
			if ((object)refConsolePlayer != null)
			{
				refConsolePlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, false);
			}
			OrangeConsoleCharacter refConsolePlayer2 = _refConsolePlayer;
			if ((object)refConsolePlayer2 != null)
			{
				refConsolePlayer2.ClearVirtualButtonStick(VirtualButtonId.SKILL0);
			}
			break;
		}
		}
	}

	protected override void OnLogicUpdate()
	{
		ForceUpdateVirtualButtonAnalog();
	}

    [Obsolete]
    protected override void SetSkillEnd()
	{
		CollideBullet bulletCollider = _refEntity.BulletCollider;
		bulletCollider.HitCallback = (CallbackObj)Delegate.Remove(bulletCollider.HitCallback, new CallbackObj(OnBulletColliderHit));
		base.OnHitPauseStateChangedEvent -= OnHitPauseChangedEvent;
		RecycleRushCollideBullet();
		_isSyncRushSkillCompleted = false;
		_skill1RushTarget = null;
		_skill1HitTransform = null;
		ParticleSystem skill0TornadoFx = _skill0TornadoFx;
		if ((object)skill0TornadoFx != null)
		{
			skill0TornadoFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		if (!_skill1ShungokusatsuBullet.bIsEnd)
		{
			_skill1ShungokusatsuBullet.BackToPool();
		}
		if ((bool)_skill1ShungokusatsuFx)
		{
			_skill1ShungokusatsuFx.BackToPool();
			_skill1ShungokusatsuFx = null;
		}
		ParticleSystem skill1RushFx = _skill1RushFx;
		if ((object)skill1RushFx != null)
		{
			skill1RushFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		ParticleSystem skill1PunchFx = _skill1PunchFx;
		if ((object)skill1PunchFx != null)
		{
			skill1PunchFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		if (_refEntity.IsLocalPlayer && _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(-1);
		}
		base.SetSkillEnd();
	}

	public override void SetRushBullet(RushCollideBullet rushCollideBullet)
	{
		_skill1RushCollideBullet = rushCollideBullet;
		if (_refEntity.UsingVehicle)
		{
			_skill1RushCollideBullet.BackToPool();
			_skill1RushCollideBullet = null;
		}
	}

	public override void SyncSkillDirection(Vector3 dir, IAimTarget target)
	{
		if (_refEntity.UsingVehicle)
		{
			if ((bool)_skill1RushCollideBullet)
			{
				_skill1RushCollideBullet.BackToPool();
				_skill1RushCollideBullet = null;
			}
			_skill1RushTarget = null;
		}
		else if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1)
		{
			SetSkill1RushVelocity(dir);
			_skill1RushTarget = target;
		}
	}
}
