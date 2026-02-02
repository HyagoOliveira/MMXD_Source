#define RELEASE
using System;
using StageLib;
using UnityEngine;

public class Event5_Human2Controller : EnemyHumanSwordController
{
	private enum IdleAnimation
	{
		SPECMOTION01 = 0,
		SPECMOTION02 = 1,
		SPECMOTION03 = 2,
		MAX_ANI = 3
	}

	[SerializeField]
	private bool CanMove;

	private int EventID = -1;

	private Transform LineUpObj;

	private FxBase LineUpFx;

	private ObjInfoBar InfoBar;

	private int AngryFrame = 80;

	private int WaitAngryFrame;

	private bool HasUseSkill;

	[SerializeField]
	protected float CheckMoveFallDownDis = 1f;

	[SerializeField]
	private float AtkMoveDis = 3f;

	private Transform ShootPos;

	private int ActionFrame;

	[SerializeField]
	private float SkillWaitTime = 1f;

	[SerializeField]
	private float Skill1UseFrame = 0.4f;

	[SerializeField]
	private float Skill1UseRate = 30f;

	private int[] _idleAnimationHash;

	private Vector3 GetShootPos
	{
		get
		{
			return ShootPos.position + Vector3.right * 1.2f * base.direction + Vector3.down * 0.15f;
		}
	}

	protected override void AwakeJob()
	{
		base.AwakeJob();
		HashIdleAnimation();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_lineup_angry_frame", 8);
	}

	private void HashIdleAnimation()
	{
		_idleAnimationHash = new int[3];
		_idleAnimationHash[0] = Animator.StringToHash("skillclip0");
		_idleAnimationHash[1] = Animator.StringToHash("skillclip1");
		_idleAnimationHash[2] = Animator.StringToHash("skillclip2");
	}

	protected override void SetStatus(int mainStatus, int subStatus = 0)
	{
		base.SetStatus(mainStatus, subStatus);
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (!CanMove && AiState == AI_STATE.mob_002)
			{
				SetStatus(MainStatus.EventIdle);
				return;
			}
			EnableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			break;
		case MainStatus.EventIdle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				CanMove = false;
				if ((bool)InfoBar)
				{
					InfoBar.gameObject.SetActive(false);
				}
				ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
				break;
			case SubStatus.Phase2:
				WaitAngryFrame = GameLogicUpdateManager.GameFrame + AngryFrame;
				if ((bool)LineUpFx)
				{
					LineUpFx.BackToPool();
				}
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					UpdateDirection();
				}
				if ((bool)LineUpObj)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_lineup_angry_frame", LineUpObj, Quaternion.identity, Array.Empty<object>());
				}
				break;
			}
			break;
		case MainStatus.SpecialMotion:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				DeActivateMeleeAttack(PlayerWeapons[0]);
				DisableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
				break;
			case SubStatus.Phase1:
				HasUseSkill = false;
				break;
			case SubStatus.Phase2:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(SkillWaitTime * 20f);
				break;
			}
			break;
		}
		UpdateAnimation();
	}

	protected override void UpdateAnimation()
	{
		base.UpdateAnimation();
		int num = 0;
		switch (_mainStatus)
		{
		case MainStatus.SpecialMotion:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				num = EnemyHumanController._animationHash[0][_isShoot];
				_animator.Play(num, 0, 0f);
				break;
			case SubStatus.Phase1:
			{
				AI_STATE aiState = AiState;
				num = _idleAnimationHash[2];
				_animator.Play(num, 0, 0f);
				break;
			}
			case SubStatus.Phase2:
				num = EnemyHumanController._animationHash[0][_isShoot];
				_animator.Play(num, 0, 0f);
				break;
			}
			break;
		case MainStatus.EventIdle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				AI_STATE aiState2 = AiState;
				num = _idleAnimationHash[0];
				_animator.Play(num, 0, 0f);
				break;
			}
			case SubStatus.Phase1:
				num = EnemyHumanController._animationHash[10][_isShoot];
				_animator.Play(num, 0, 0f);
				break;
			case SubStatus.Phase2:
			{
				AI_STATE aiState3 = AiState;
				num = _idleAnimationHash[1];
				_animator.Play(num, 0, 0f);
				break;
			}
			}
			break;
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate || !BuildDone || PlayerWeapons == null || PlayerWeapons[0].BulletData == null)
		{
			return;
		}
		PlayerWeapons[0].ChargeTimer += GameLogicUpdateManager.m_fFrameLenMS;
		PlayerWeapons[0].LastUseTimer += GameLogicUpdateManager.m_fFrameLenMS;
		BaseLogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SlashCount = 0;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					if (Mathf.Abs(Target.GetTargetPoint().x - _transform.position.x) > WalkDistance && !CheckMoveFall(_velocity + VInt3.signRight * base.direction * EnemyHumanController.WalkSpeed))
					{
						SetStatus(MainStatus.Walk);
					}
					else if (PlayerWeapons[WeaponCurrent].LastUseTimer.GetMillisecond() > PlayerWeapons[WeaponCurrent].BulletData.n_RELOAD && Mathf.Abs(Target._transform.position.y - _transform.position.y) < 1.5f)
					{
						SetStatus(MainStatus.Idle, SubStatus.Phase2);
					}
					UpdateDirection();
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase2:
				if (!(_currentFrame > 1f))
				{
					break;
				}
				SlashCount++;
				if (SlashCount >= PlayerWeapons[WeaponCurrent].BulletData.n_MAGAZINE)
				{
					Slashing = false;
					if ((float)OrangeBattleUtility.Random(0, 100) > Skill1UseRate)
					{
						SetStatus(MainStatus.Idle, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.SpecialMotion);
					}
				}
				else
				{
					SetStatus(MainStatus.Idle, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					PlayerWeapons[WeaponCurrent].LastUseTimer.TimerStart();
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Walk:
			if (CheckMoveFall(_velocity))
			{
				SetStatus(MainStatus.Idle);
				break;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Target == null || (double)Mathf.Abs(Target.GetTargetPoint().x - _transform.position.x) < (double)WalkDistance - 1.5)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Crouch:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.SpecialMotion:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.SpecialMotion, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if (!Target)
				{
					break;
				}
				if (!HasUseSkill && _currentFrame > Skill1UseFrame)
				{
					HasUseSkill = true;
					float value = Vector2.Angle(Vector2.up, Target.GetTargetPoint() + Vector3.up * 0.15f - GetShootPos);
					_animator.SetFloat(hashDirection, value);
					Vector3 pDirection = Target.GetTargetPoint() + Vector3.up * 0.15f - GetShootPos;
					if ((Target.GetTargetPoint().x - _transform.position.x) * (Target.GetTargetPoint().x - GetShootPos.x) < 0f)
					{
						value = 90f;
						_animator.SetFloat(hashDirection, value);
						pDirection = Vector3.right * base.direction;
					}
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.SpecialMotion, SubStatus.Phase2);
				}
				UpdateDirection();
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					if ((float)OrangeBattleUtility.Random(0, 100) > Skill1UseRate)
					{
						SetStatus(MainStatus.Idle);
					}
					else
					{
						SetStatus(MainStatus.SpecialMotion);
					}
				}
				break;
			}
			break;
		case MainStatus.EventIdle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (CanMove)
				{
					SetStatus(MainStatus.EventIdle, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.EventIdle);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					if ((bool)InfoBar)
					{
						InfoBar.gameObject.SetActive(true);
					}
					SetColliderEnable(true);
					base.AllowAutoAim = true;
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		}
	}

	public override void SetActiveReal(bool isActive)
	{
		base.SetActiveReal(isActive);
		CanMove = false;
		if (isActive)
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, StartMove);
			DisableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			LineUpObj = OrangeBattleUtility.FindChildRecursive(base.transform, "LineUp");
			ShootPos = OrangeBattleUtility.FindChildRecursive(base.transform, "L BusterPoint");
			if ((bool)LineUpObj)
			{
				switch (AiState)
				{
				case AI_STATE.mob_001:
					LineUpFx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_lineup_heart_frame", LineUpObj, Quaternion.identity, Array.Empty<object>());
					break;
				}
			}
			InfoBar = _transform.GetComponentInChildren<ObjInfoBar>();
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_002)
			{
				base.AllowAutoAim = false;
				SetColliderEnable(false);
				SetStatus(MainStatus.EventIdle);
			}
			else
			{
				base.AllowAutoAim = true;
				SetColliderEnable(true);
				SetStatus(MainStatus.Idle);
			}
		}
		else
		{
			if ((bool)LineUpFx)
			{
				LineUpFx.BackToPool();
			}
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, StartMove);
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		base.SetPositionAndRotation(pos, bBack);
	}

	public void StartMove(EventManager.StageEventCall tStageEventCall)
	{
		if (EventID == -1 || tStageEventCall.nID != EventID)
		{
			return;
		}
		CanMove = true;
		EnableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
		for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
		{
			if (StageUpdate.runEnemys[i].mEnemy.gameObject.GetInstanceID() == base.gameObject.GetInstanceID())
			{
				StageUpdate.runEnemys[i].nEnemyBitParam = StageUpdate.runEnemys[i].nEnemyBitParam | 1;
			}
		}
		SetStatus(MainStatus.EventIdle, SubStatus.Phase2);
	}

	public override void SetEventCtrlID(int eventid)
	{
		EventID = eventid;
	}

	protected override bool CheckMoveFall(VInt3 velocity)
	{
		if (!DisableMoveFall)
		{
			return false;
		}
		float y = Controller.GetBounds().size.y;
		VInt3 vInt = velocity * GameLogicUpdateManager.m_fFrameLen;
		float num = Mathf.Abs(vInt.vec3.x);
		int num2 = Math.Sign(vInt.vec3.x);
		Controller2D.RaycastOrigins raycastOrigins = Controller.GetRaycastOrigins();
		Vector2 vector = ((num2 == -1) ? raycastOrigins.topLeft : raycastOrigins.topRight);
		if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector2.right * num2, num, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough, _transform))
		{
			return false;
		}
		Vector2 vector2 = vector + Vector2.right * num2 * num;
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector2, Vector2.down, y + CheckMoveFallDownDis, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough, _transform);
		Debug.DrawLine(vector, vector2, Color.cyan, 0.5f);
		if ((bool)raycastHit2D)
		{
			Debug.DrawLine(vector2, raycastHit2D.point, Color.cyan, 0.5f);
			return false;
		}
		Debug.DrawLine(vector2, vector2 + Vector2.down * (y + CheckMoveFallDownDis), Color.cyan, 0.5f);
		return true;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		WalkDistance = AtkMoveDis;
	}

	public override void Unlock()
	{
		_unlockReady = true;
		base.AllowAutoAim = true;
		AI_STATE aiState = AiState;
		if (aiState != AI_STATE.mob_002)
		{
			SetColliderEnable(true);
		}
		if (InGame && (int)Hp > 0)
		{
			Activate = true;
		}
	}
}
