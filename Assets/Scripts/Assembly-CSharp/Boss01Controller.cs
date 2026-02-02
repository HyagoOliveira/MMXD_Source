using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using RootMotion.FinalIK;
using StageLib;
using UnityEngine;

public class Boss01Controller : EnemyControllerBase, IManagedLateUpdateBehavior
{
	public enum MainStatus
	{
		Intro = 0,
		Idle = 1,
		LeftPunch = 2,
		RightPunch = 3,
		LeftSwing = 4,
		RightSwing = 5,
		SummonLeftSwing = 6,
		SummonRightSwing = 7,
		Dead = 8,
		IdleWaitNet = 9,
		MAX_STATUS = 10
	}

	public enum SubStatus
	{
		Predict = 0,
		Normal = 1,
		Normal2 = 2,
		End = 3,
		MAX_SUBSTATUS = 4
	}

	public enum DieStatus
	{
		Left = 0,
		Right = 1,
		Mid = 2,
		End = 3,
		MAX_SUBSTATUS = 4
	}

	public enum AnimationID
	{
		ANI_INTRO = 0,
		ANI_IDLE = 1,
		ANI_LEFT_PUNCH = 2,
		ANI_LEFT_PUNCH_END = 3,
		ANI_RIGHT_PUNCH = 4,
		ANI_RIGHT_PUNCH_END = 5,
		ANI_RIGHT_SWING = 6,
		ANI_RIGHT_SWING_END = 7,
		ANI_LEFT_SWING = 8,
		ANI_LEFT_SWING_END = 9,
		ANI_DEAD = 10,
		ANI_DEAD_END = 11,
		MAX_ANIMATION_ID = 12
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private DieStatus _DieStatus;

	private Transform _swingTarget;

	private OrangeTimer PredictTimer;

	private SpriteRenderer PredictSpriteRenderer;

	private const long PredictTimeout = 500L;

	private Transform R_HitPoint;

	[SerializeField]
	private Sprite PunchPredictSprite;

	[SerializeField]
	private Sprite SwingPredictSprite;

	[SerializeField]
	private AimIK HeadIK;

	[SerializeField]
	private AimIK RightIK;

	[SerializeField]
	private AimIK LeftIK;

	private AnimationID _currentAnimationId;

	private Boss01BallBullet LBall;

	private Boss01BallBullet RBall;

	private float _currentFrame;

	private int[] _animationHash;

	private readonly Quaternion _quaternionNormal = Quaternion.Euler(0f, 0f, 0f);

	private readonly Quaternion _quaternionReverse = Quaternion.Euler(0f, 180f, 0f);

	private readonly string FX_BOSS_EXPLODE1 = "FX_BOSS_EXPLODE1";

	private readonly string FX_BOSS_EXPLODE2 = "FX_BOSS_EXPLODE2";

	private Transform L_Explode1;

	private Transform R_Explode1;

	private Transform H_Explode;

	private bool IsChipInfoAnim;

	private bool _bDeadCallResult = true;

	[SerializeField]
	private float summonTime = 1f;

	private bool isPlaySE02;

	private bool init;

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Predict)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		default:
			_transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
			break;
		case MainStatus.LeftSwing:
			if (_subStatus == SubStatus.Predict)
			{
				_transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
			}
			else
			{
				_transform.localScale = new Vector3(0.4f, 0.4f, -0.4f);
			}
			break;
		case MainStatus.SummonLeftSwing:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Normal)
			{
				_transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
			}
			else
			{
				_transform.localScale = new Vector3(0.4f, 0.4f, -0.4f);
			}
			break;
		}
		case MainStatus.Dead:
			OrangeBattleUtility.LockPlayer();
			break;
		}
		if (IsChipInfoAnim)
		{
			_transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		}
		AiTimer.TimerStart();
		UpdatePredictor();
		UpdateCollider();
		UpdateIKSetting();
		UpdateAnimation();
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
	}

	protected override void Start()
	{
		Init();
	}

	private void Init()
	{
		if (!init)
		{
			init = true;
			base.Start();
			if (null == _enemyAutoAimSystem)
			{
				OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
			}
			Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
			int num = 1;
			StageObjParam stageObjParam = OrangeBattleUtility.FindChildRecursive(ref target, "L_Collider", true).gameObject.AddOrGetComponent<StageObjParam>();
			stageObjParam.nSubPartID = num++;
			GuardTransform.Add(stageObjParam.nSubPartID);
			stageObjParam = OrangeBattleUtility.FindChildRecursive(ref target, "R_Collider", true).gameObject.AddOrGetComponent<StageObjParam>();
			stageObjParam.nSubPartID = num++;
			GuardTransform.Add(stageObjParam.nSubPartID);
			base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Collider", true);
			_swingTarget = OrangeBattleUtility.FindChildRecursive(ref target, "SwingTarget", true);
			PredictTimer = OrangeTimerManager.GetTimer();
			PredictSpriteRenderer = _swingTarget.GetComponent<SpriteRenderer>();
			PredictSpriteRenderer.enabled = false;
			_animator = GetComponent<Animator>();
			_bDeadPlayCompleted = false;
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_BOSS_EXPLODE1, 2);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_BOSS_EXPLODE2);
			L_Explode1 = OrangeBattleUtility.FindChildRecursive(ref target, "L_Explode1", true);
			R_Explode1 = OrangeBattleUtility.FindChildRecursive(ref target, "R_Explode1", true);
			H_Explode = OrangeBattleUtility.FindChildRecursive(ref target, "H_Explode", true);
			LBall = OrangeBattleUtility.FindChildRecursive(ref target, "L_HitPoint", true).gameObject.AddOrGetComponent<Boss01BallBullet>();
			R_HitPoint = OrangeBattleUtility.FindChildRecursive(ref target, "R_HitPoint", true);
			RBall = R_HitPoint.gameObject.AddOrGetComponent<Boss01BallBullet>();
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_maoh_001", 10);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget", 2);
			_enemyAutoAimSystem.UpdateAimRange(100f);
			_animationHash = new int[12];
			_animationHash[0] = Animator.StringToHash("Intro");
			_animationHash[1] = Animator.StringToHash("Idle");
			_animationHash[2] = Animator.StringToHash("LeftPunch");
			_animationHash[3] = Animator.StringToHash("LeftPunchEnd");
			_animationHash[4] = Animator.StringToHash("RightPunch");
			_animationHash[5] = Animator.StringToHash("RightPunchEnd");
			_animationHash[8] = Animator.StringToHash("LeftSwing");
			_animationHash[9] = Animator.StringToHash("LeftSwingEnd");
			_animationHash[6] = Animator.StringToHash("RightSwing");
			_animationHash[7] = Animator.StringToHash("RightSwingEnd");
			_animationHash[10] = Animator.StringToHash("Dead");
			_animationHash[11] = Animator.StringToHash("DeadEnd");
			if (IsChipInfoAnim)
			{
				SetStatus(MainStatus.Idle);
			}
			else
			{
				SetStatus(MainStatus.Intro);
			}
			if ((bool)_characterMaterial)
			{
				_characterMaterial.HurtColor = new Color(0.43f, 0.43f, 0.43f, 0.65f);
			}
		}
	}

	public override void SetChipInfoAnim()
	{
		IsChipInfoAnim = true;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		AI_STATE aiState = AI_STATE.mob_001;
		if (EnemyData.s_AI != "null")
		{
			aiState = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
		}
		AiState = aiState;
		AI_STATE aiState2 = AiState;
		if (aiState2 == AI_STATE.mob_002)
		{
			_bDeadCallResult = false;
		}
		else
		{
			_bDeadCallResult = true;
		}
	}

	public override void SetActive(bool isActive)
	{
		InGame = isActive;
		Activate = isActive;
		base.AllowAutoAim = false;
		if (isActive)
		{
			if (LBall == null)
			{
				Init();
			}
			MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, true);
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		}
		else
		{
			LeanTween.cancel(base.gameObject);
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
			MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, false);
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
		_transform.position = new Vector3(pos.x, pos.y, 12f);
		Controller.LogicPosition = new VInt3(_transform.position);
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.LeftPunch:
			switch (_subStatus)
			{
			case SubStatus.Predict:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Normal:
				_currentAnimationId = AnimationID.ANI_LEFT_PUNCH;
				break;
			case SubStatus.End:
				_currentAnimationId = AnimationID.ANI_LEFT_PUNCH_END;
				break;
			}
			break;
		case MainStatus.RightPunch:
			switch (_subStatus)
			{
			case SubStatus.Predict:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Normal:
				_currentAnimationId = AnimationID.ANI_RIGHT_PUNCH;
				break;
			case SubStatus.End:
				_currentAnimationId = AnimationID.ANI_RIGHT_PUNCH_END;
				break;
			}
			break;
		case MainStatus.LeftSwing:
		case MainStatus.RightSwing:
			switch (_subStatus)
			{
			case SubStatus.Predict:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Normal:
				_currentAnimationId = AnimationID.ANI_RIGHT_SWING;
				break;
			case SubStatus.End:
				_currentAnimationId = AnimationID.ANI_RIGHT_SWING_END;
				break;
			}
			break;
		case MainStatus.SummonLeftSwing:
		case MainStatus.SummonRightSwing:
			switch (_subStatus)
			{
			case SubStatus.Predict:
			case SubStatus.Normal:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Normal2:
				_currentAnimationId = AnimationID.ANI_RIGHT_SWING;
				break;
			case SubStatus.End:
				_currentAnimationId = AnimationID.ANI_RIGHT_SWING_END;
				break;
			}
			break;
		case MainStatus.Dead:
			if (_subStatus == SubStatus.Predict)
			{
				_currentAnimationId = AnimationID.ANI_DEAD;
			}
			else
			{
				_currentAnimationId = AnimationID.ANI_DEAD_END;
			}
			break;
		case MainStatus.IdleWaitNet:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdatePredictor()
	{
		switch (_subStatus)
		{
		case SubStatus.Predict:
			switch (_mainStatus)
			{
			case MainStatus.LeftPunch:
			case MainStatus.RightPunch:
			{
				RaycastHit2D raycastHit2D = Physics2D.Raycast(TargetPos.vec3, Vector2.down, 10f, BulletScriptableObject.Instance.BulletLayerMaskObstacle);
				if ((double)raycastHit2D.distance < 1.0)
				{
					_swingTarget.position = new Vector3(TargetPos.vec3.x, raycastHit2D.point.y, 0f);
					_swingTarget.localRotation = Quaternion.Euler(90f, 90f, 0f);
				}
				else
				{
					_swingTarget.position = TargetPos.vec3;
					_swingTarget.rotation = Quaternion.Euler(0f, 0f, 0f);
				}
				_swingTarget.localScale = Vector3.one * 3f;
				PredictSpriteRenderer.sprite = PunchPredictSprite;
				PredictSpriteRenderer.enabled = true;
				PredictTimer.TimerStart();
				break;
			}
			case MainStatus.LeftSwing:
			case MainStatus.RightSwing:
				PredictSwingAttack();
				PredictTimer.TimerStart();
				break;
			case MainStatus.SummonLeftSwing:
			case MainStatus.SummonRightSwing:
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
				PredictTimer.TimerStart();
				break;
			}
			break;
		case SubStatus.Normal:
		{
			MainStatus mainStatus = _mainStatus;
			if ((uint)(mainStatus - 6) <= 1u)
			{
				PredictSwingAttack();
				PredictTimer.TimerStart();
			}
			else
			{
				PredictSpriteRenderer.enabled = false;
			}
			break;
		}
		default:
			PredictSpriteRenderer.enabled = false;
			break;
		}
	}

	private void PredictSwingAttack()
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(TargetPos.vec3, Vector2.down, 10f, BulletScriptableObject.Instance.BulletLayerMaskObstacle);
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		if (_mainStatus == MainStatus.LeftSwing || _mainStatus == MainStatus.SummonLeftSwing)
		{
			zero = new Vector3(_transform.position.x + 8.3f, raycastHit2D.point.y + 1.26f, 0f);
			zero2 = new Vector3(_transform.position.x - 20f, raycastHit2D.point.y + 1.26f, 0f);
		}
		else
		{
			zero = new Vector3(_transform.position.x - 8.3f, raycastHit2D.point.y + 1.26f, 0f);
			zero2 = new Vector3(_transform.position.x + 20f, raycastHit2D.point.y + 1.26f, 0f);
		}
		float distance = Vector2.Distance(zero, zero2);
		Vector3 vector = (zero.xy() - zero2.xy()).normalized;
		MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", zero, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, -vector)), Array.Empty<object>()).SetEffect(distance, new Color(1f, 1f, 0f, 0.6f), new Color(1f, 0.54f, 0f), 1f, 3f);
	}

	private void UpdateCollider()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Dead:
			if (LBall.IsActivate)
			{
				LBall.IsDestroy = true;
			}
			if (RBall.IsActivate)
			{
				RBall.IsDestroy = true;
			}
			break;
		case MainStatus.LeftPunch:
			switch (_subStatus)
			{
			case SubStatus.Normal:
				LBall.UpdateBulletData(EnemyWeapons[0].BulletData);
				LBall.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				LBall.Active(targetMask, 0);
				break;
			case SubStatus.Predict:
			case SubStatus.End:
				if (LBall.IsActivate)
				{
					LBall.IsDestroy = true;
				}
				break;
			case SubStatus.Normal2:
				break;
			}
			break;
		case MainStatus.RightPunch:
		case MainStatus.LeftSwing:
		case MainStatus.RightSwing:
			switch (_subStatus)
			{
			case SubStatus.Normal:
				RBall.UpdateBulletData(ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[EnemyWeapons[(int)(_mainStatus - 2)].BulletData.n_ID]);
				RBall.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				RBall.Active(targetMask, (_mainStatus != MainStatus.RightPunch) ? 1 : 0);
				break;
			case SubStatus.Predict:
			case SubStatus.End:
				if (RBall.IsActivate)
				{
					RBall.IsDestroy = true;
				}
				break;
			case SubStatus.Normal2:
				break;
			}
			break;
		case MainStatus.SummonLeftSwing:
		case MainStatus.SummonRightSwing:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Normal2)
			{
				int num = (int)(_mainStatus - 2);
				RBall.UpdateBulletData(ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[EnemyWeapons[num - 2].BulletData.n_ID]);
				RBall.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				RBall.Active(targetMask, (_mainStatus != MainStatus.RightPunch) ? 1 : 0);
			}
			else if (RBall.IsActivate)
			{
				RBall.IsDestroy = true;
			}
			break;
		}
		}
	}

	private void UpdateIKSetting()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if ((bool)Target)
			{
				HeadIK.solver.target = Target.transform;
				LeanTween.value(base.gameObject, HeadIK.solver.IKPositionWeight, 1f, 0.2f).setOnUpdate(delegate(float f)
				{
					HeadIK.solver.IKPositionWeight = f;
				});
			}
			else
			{
				LeanTween.value(base.gameObject, HeadIK.solver.IKPositionWeight, 0f, 0.2f).setOnUpdate(delegate(float f)
				{
					HeadIK.solver.IKPositionWeight = f;
				});
			}
			break;
		case MainStatus.LeftPunch:
		case MainStatus.RightPunch:
			HeadIK.solver.target = _swingTarget;
			break;
		case MainStatus.LeftSwing:
		case MainStatus.RightSwing:
			if (_subStatus != 0)
			{
				HeadIK.solver.IKPositionWeight = 0f;
				LeftIK.solver.IKPositionWeight = 0f;
				RightIK.solver.IKPositionWeight = 0f;
			}
			break;
		case MainStatus.SummonLeftSwing:
		case MainStatus.SummonRightSwing:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus != SubStatus.Normal)
			{
				HeadIK.solver.IKPositionWeight = 0f;
				LeftIK.solver.IKPositionWeight = 0f;
				RightIK.solver.IKPositionWeight = 0f;
			}
			break;
		}
		}
	}

	public void LateUpdateFunc()
	{
		if (!Activate || !_enemyAutoAimSystem)
		{
			return;
		}
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		IgnoreGravity = true;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (_currentFrame > 1f)
			{
				UpdateRandomState();
			}
			break;
		case MainStatus.LeftPunch:
			switch (_subStatus)
			{
			case SubStatus.Predict:
				if (PredictTimer.GetMillisecond() > 500)
				{
					SetStatus((_swingTarget.position.x < _transform.position.x) ? MainStatus.RightPunch : MainStatus.LeftPunch, SubStatus.Normal);
				}
				break;
			case SubStatus.Normal:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.End);
				}
				else
				{
					LeftIK.solver.IKPositionWeight = Mathf.Min(_currentFrame * 3.6630037f, 1f);
				}
				break;
			case SubStatus.End:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
					LeftIK.solver.IKPositionWeight = 0f;
				}
				else
				{
					LeftIK.solver.IKPositionWeight = 1f - _currentFrame;
				}
				break;
			case SubStatus.Normal2:
				break;
			}
			break;
		case MainStatus.RightPunch:
			switch (_subStatus)
			{
			case SubStatus.Predict:
				if (PredictTimer.GetMillisecond() > 500)
				{
					SetStatus((_swingTarget.position.x < _transform.position.x) ? MainStatus.RightPunch : MainStatus.LeftPunch, SubStatus.Normal);
				}
				break;
			case SubStatus.Normal:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.End);
				}
				else
				{
					RightIK.solver.IKPositionWeight = Mathf.Min(_currentFrame * 2.3640661f, 1f);
				}
				break;
			case SubStatus.End:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
					RightIK.solver.IKPositionWeight = 0f;
				}
				else
				{
					RightIK.solver.IKPositionWeight = 1f - _currentFrame;
				}
				break;
			case SubStatus.Normal2:
				break;
			}
			break;
		case MainStatus.LeftSwing:
		case MainStatus.RightSwing:
			switch (_subStatus)
			{
			case SubStatus.Predict:
				if (PredictTimer.GetMillisecond() > 500)
				{
					SetStatus(_mainStatus, SubStatus.Normal);
				}
				break;
			case SubStatus.Normal:
			{
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.End);
				}
				if (!((double)_currentFrame > 0.47))
				{
					break;
				}
				if (!isPlaySE02)
				{
					base.SoundSource.PlaySE("BossSE", 2);
					isPlaySE02 = true;
				}
				RaycastHit2D raycastHit2D2 = Physics2D.Raycast(R_HitPoint.position, Vector2.down, 3f, BulletScriptableObject.Instance.BulletLayerMaskObstacle);
				if ((bool)raycastHit2D2)
				{
					Vector2 vector2 = new Vector2(raycastHit2D2.point.x, raycastHit2D2.point.y);
					if (MainStatus.LeftSwing == _mainStatus)
					{
						vector2.x -= 3f;
					}
					else
					{
						vector2.x += 3f;
					}
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_maoh_001", vector2, (_mainStatus == MainStatus.LeftSwing) ? _quaternionReverse : _quaternionNormal, Array.Empty<object>());
				}
				break;
			}
			case SubStatus.End:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
					isPlaySE02 = false;
				}
				break;
			case SubStatus.Normal2:
				break;
			}
			break;
		case MainStatus.SummonLeftSwing:
		case MainStatus.SummonRightSwing:
			switch (_subStatus)
			{
			case SubStatus.Predict:
				if ((float)PredictTimer.GetMillisecond() > summonTime)
				{
					SetStatus(_mainStatus, SubStatus.Normal);
				}
				break;
			case SubStatus.Normal:
				if (PredictTimer.GetMillisecond() > 500)
				{
					SetStatus(_mainStatus, SubStatus.Normal2);
				}
				break;
			case SubStatus.Normal2:
			{
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.End);
				}
				if (!((double)_currentFrame > 0.47))
				{
					break;
				}
				if (!isPlaySE02)
				{
					base.SoundSource.PlaySE("BossSE", 2);
					isPlaySE02 = true;
				}
				RaycastHit2D raycastHit2D = Physics2D.Raycast(R_HitPoint.position, Vector2.down, 3f, BulletScriptableObject.Instance.BulletLayerMaskObstacle);
				if ((bool)raycastHit2D)
				{
					Vector2 vector = new Vector2(raycastHit2D.point.x, raycastHit2D.point.y);
					if (MainStatus.LeftSwing == _mainStatus)
					{
						vector.x -= 3f;
					}
					else
					{
						vector.x += 3f;
					}
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_maoh_001", vector, (_mainStatus == MainStatus.LeftSwing) ? _quaternionReverse : _quaternionNormal, Array.Empty<object>());
				}
				break;
			}
			case SubStatus.End:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
					isPlaySE02 = false;
				}
				break;
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Predict:
				switch (_DieStatus)
				{
				case DieStatus.Left:
					if (_currentFrame > 0.05f)
					{
						base.SoundSource.PlaySE("HitSE", 102);
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_BOSS_EXPLODE1, L_Explode1, Quaternion.identity, Array.Empty<object>());
						_DieStatus = DieStatus.Right;
					}
					break;
				case DieStatus.Right:
					if (_currentFrame > 0.35f)
					{
						base.SoundSource.PlaySE("HitSE", 103);
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_BOSS_EXPLODE1, R_Explode1, Quaternion.identity, Array.Empty<object>());
						_DieStatus = DieStatus.Mid;
					}
					break;
				case DieStatus.Mid:
					if (_currentFrame > 0.8f)
					{
						_DieStatus = DieStatus.End;
						if (_bDeadCallResult)
						{
							StartCoroutine(BossDieFlow(H_Explode));
						}
						else
						{
							StartCoroutine(BossDieFlow(H_Explode, "FX_BOSS_EXPLODE2", false, false));
						}
					}
					break;
				case DieStatus.End:
					if (_currentFrame > 1f)
					{
						SetStatus(_mainStatus, SubStatus.Normal);
					}
					break;
				}
				break;
			case SubStatus.Normal:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.End);
				}
				break;
			case SubStatus.End:
				if (AiTimer.GetMillisecond() > 1300)
				{
					AiTimer.TimerStop();
				}
				break;
			case SubStatus.Normal2:
				break;
			}
			break;
		case MainStatus.Intro:
		case MainStatus.IdleWaitNet:
			break;
		}
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			mainStatus = (MainStatus)UnityEngine.Random.Range(1, 6);
			if (AiState == AI_STATE.mob_002)
			{
				switch (mainStatus)
				{
				case MainStatus.LeftSwing:
					mainStatus = MainStatus.SummonLeftSwing;
					break;
				case MainStatus.RightSwing:
					mainStatus = MainStatus.SummonRightSwing;
					break;
				}
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				TargetPos = new VInt3(Target.transform.position);
			}
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
		}
		if (StageUpdate.bIsHost)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.TargetPosX = TargetPos.x;
			netSyncData.TargetPosY = TargetPos.y;
			netSyncData.TargetPosZ = TargetPos.z;
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus, JsonConvert.SerializeObject(netSyncData));
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (smsg.Equals("GG"))
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				TargetPos = new VInt3(Target.transform.position);
			}
		}
		else if (smsg != null && smsg != "")
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
			Controller.LogicPosition.x = netSyncData.SelfPosX;
			Controller.LogicPosition.y = netSyncData.SelfPosY;
			Controller.LogicPosition.z = netSyncData.SelfPosZ;
			TargetPos.x = netSyncData.TargetPosX;
			TargetPos.y = netSyncData.TargetPosY;
			TargetPos.z = netSyncData.TargetPosZ;
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
		}
		SetStatus((MainStatus)nSet);
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (GuardTransform.Contains(tHurtPassParam.nSubPartID))
		{
			return Hp;
		}
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg, base.UpdateHurtAction);
		if ((int)Hp > 0)
		{
			_characterMaterial.Hurt();
		}
		else if (_mainStatus != MainStatus.Dead)
		{
			StageUpdate.SlowStage();
			SetStatus(MainStatus.Dead);
		}
		return Hp;
	}

	public override void BossIntro(Action cb)
	{
		base.SoundSource.PlaySE("BossSE", 3);
		LeanTween.value(base.gameObject, _transform.position.y, _transform.position.y + 15.05f, 4f).setOnUpdate(delegate(float f)
		{
			Vector3 position = _transform.position;
			position.y = f;
			_transform.position = position;
		}).setOnComplete((Action<object>)delegate
		{
			cb();
		});
	}

	public override void enemylock()
	{
		if (InGame)
		{
			Activate = false;
			GetComponent<Animator>().enabled = false;
		}
	}

	public override void Unlock()
	{
		base.Unlock();
		if (_mainStatus == MainStatus.Intro)
		{
			SetStatus(MainStatus.Idle);
			AiTimer.TimerStart();
		}
		GetComponent<Animator>().enabled = true;
	}

	public override Vector2 GetDamageTextPos()
	{
		return base.transform.position.xy() + new Vector2(0f, 6.5f);
	}
}
