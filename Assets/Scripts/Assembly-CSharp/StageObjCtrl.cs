using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using StageLib;
using UnityEngine;

public class StageObjCtrl : EnemyControllerBase, IManagedUpdateBehavior
{
	[Serializable]
	public class AIData
	{
		public int nStatus;

		public int nRand;
	}

	[Serializable]
	public class AIRangeData
	{
		public float fRange;

		public List<AIData> listAIData = new List<AIData>();
	}

	public const string NowStatusStr = "NowStatus";

	public const string BelowStr = "bBelow";

	public const string SpeedYStr = "nSpeedY";

	public const int BossWeight = 9;

	[SerializeField]
	private List<Transform> ModelBody = new List<Transform>();

	public static string[] eventnames = new string[22]
	{
		"0.無", "1.觸發怪物介紹", "2.取得下個動作", "3.開啟衝刺攻擊偵測", "4.衝刺移動", "5.設定跳躍速度", "6.開啟攻擊1偵測", "7.關閉攻擊1偵測", "8.攻擊1發射", "9.開啟攻擊2偵測",
		"10.關閉攻擊2偵測", "11.攻擊2發射", "12.開啟攻擊3偵測", "13.關閉攻擊3偵測", "14.攻擊3發射", "15.爆炸白幕勝利動作", "16.爆炸白幕", "17.呼叫動畫CB", "18.關閉武器", "19.移動歸零",
		"20.開啟落下攻擊", "21.關閉落下攻擊"
	};

	public static string[] eventfuncname = new string[22]
	{
		"", "CallIntro", "GetNextStatusByAI", "RunAtkOn", "RunMoveUpdate", "SetJumpSpeed", "Atk1On", "Atk1Off", "Atk1Shot", "Atk2On",
		"Atk2Off", "Atk2Shot", "Atk3On", "Atk3Off", "Atk3Shot", "ExploderEffectAndPos", "ExploderEffect", "CallAnimationCB", "DisableCurrentWeapon", "MoveSpeedZero",
		"JumpDownAtkOn", "JumpDownAtkOff"
	};

	private STAGE_MODEL_ANIMATION _mainStatus;

	public int JumpXSpeed = 10;

	public int JumpYSpeed = 80;

	public int RunSpeed = 20;

	public string RunFxName = "";

	public int RndEndStatus;

	public string JumpDownFxName = "";

	public int AILevel = 1;

	private Vector3 lock_Pos = new Vector3(0f, 0f, 0f);

	private Vector3 source_Pos = new Vector3(0f, 0f, 0f);

	public List<AIRangeData> listAIRD = new List<AIRangeData>();

	private long nMaxAtkRange;

	private Transform LeftAtk;

	private Transform RightAtk;

	private bool DebutUpdateDirection;

	private Action IntoBack;

	private int _shootNum;

	private CollideBullet ClawCollideBullet;

	private CollideBullet Jump_Run_CollideBullet;

	private float _currentFrame;

	private int[] _animationHash;

	private Transform ExplosionRoot;

	private Transform[] ShotPointFs = new Transform[3];

	private Transform[] ShotPointBs = new Transform[3];

	private Vector3 defaultModelRotation = new Vector3(0f, 90f, 0f);

	private bool _bDeadCallResult = true;

	private bool _bAlwatsStand;

	private float fNowWaitTime;

	public float fNextWaitTime = 1f;

	private bool CanCallEvent;

	private int FrameCount;

	private bool StartCount;

	private bool callright = true;

	private int rightTime = 2;

	private int leftTime = 22;

	private int loopTime = 40;

	private float StageLiftPosX;

	private float StageRightPosX;

	private bool bAlwaysStand
	{
		get
		{
			return _bAlwatsStand;
		}
		set
		{
			if (_bAlwatsStand == value)
			{
				return;
			}
			_bAlwatsStand = value;
			if (_bAlwatsStand)
			{
				if (_animator == null)
				{
					_animator = GetComponentInChildren<Animator>();
				}
				_animator.SetBool("bBelow", true);
				MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
			}
		}
	}

	private void OnEnable()
	{
		if (!bAlwaysStand)
		{
			MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Start()
	{
		base.Start();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "AimPoint");
		_animator = GetComponentInChildren<Animator>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, "ShootPoint");
		int num = 0;
		for (int i = 0; i < ShotPointFs.Length; i++)
		{
			ShotPointFs[i] = null;
			ShotPointBs[i] = null;
		}
		Transform[] array2 = array;
		foreach (Transform transform in array2)
		{
			if (transform.gameObject.name.StartsWith("ShootPointF"))
			{
				num = int.Parse(transform.gameObject.name.Substring("ShootPointF".Length));
				ShotPointFs[num - 1] = transform;
			}
			else if (transform.gameObject.name.StartsWith("ShootPointB"))
			{
				num = int.Parse(transform.gameObject.name.Substring("ShootPointB".Length));
				ShotPointBs[num - 1] = transform;
			}
		}
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "ColliderBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		_collideBullet.isForceSE = (_collideBullet.isBossBullet = true);
		ClawCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "ClawCollideBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		ClawCollideBullet.isForceSE = (ClawCollideBullet.isBossBullet = true);
		Jump_Run_CollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "ColliderBullet_Jump_Run", true).gameObject.AddOrGetComponent<CollideBullet>();
		Jump_Run_CollideBullet.isForceSE = (Jump_Run_CollideBullet.isBossBullet = true);
		ExplosionRoot = OrangeBattleUtility.FindChildRecursive(ref target, "ExplosionRoot", true);
		if (AILevel == 1)
		{
			if (null == _enemyAutoAimSystem)
			{
				OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
				_enemyAutoAimSystem.UpdateAimRange(100f);
			}
			if (RunFxName != "")
			{
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(RunFxName, 10);
			}
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_maoh_000", 2);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
			LeftAtk = OrangeBattleUtility.FindChildRecursive(ref target, "L_hand_bone4", true);
			RightAtk = OrangeBattleUtility.FindChildRecursive(ref target, "R_hand_bone4", true);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget", 2);
		}
		base.direction = 1;
		preDirection = base.direction;
		IgnoreGravity = false;
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
		if (AILevel == 1)
		{
			DebutUpdateDirection = false;
		}
		nMaxAtkRange = 0L;
		foreach (AIRangeData item in listAIRD)
		{
			if (item.fRange * 1000f > (float)nMaxAtkRange)
			{
				nMaxAtkRange = (int)(item.fRange * 1000f);
			}
		}
		nMaxAtkRange *= nMaxAtkRange;
	}

	public override void SetChipInfoAnim()
	{
		bAlwaysStand = true;
	}

	private void SetStatus(STAGE_MODEL_ANIMATION mainStatus)
	{
		_mainStatus = mainStatus;
		_animator.SetInteger("NowStatus", (int)_mainStatus);
		AiTimer.TimerStart();
		UpdateCollider();
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if (smsg != null && smsg != "")
		{
			string[] array = smsg.Split(',');
			int num = 0;
			while (num < array.Length)
			{
				switch (array[num++])
				{
				case "TargetPos":
					TargetPos.x = int.Parse(array[num++]);
					TargetPos.y = int.Parse(array[num++]);
					break;
				case "Hp":
					Hp = int.Parse(array[num++]);
					DmgHp = (int)MaxHp - (int)Hp;
					HealHp = 0;
					break;
				case "SelfPos":
					Controller.LogicPosition.x = int.Parse(array[num++]);
					Controller.LogicPosition.y = int.Parse(array[num++]);
					Controller.LogicPosition.z = int.Parse(array[num++]);
					break;
				}
			}
			UpdateDirection();
		}
		tAnimationCB = tCB;
		SetStatus((STAGE_MODEL_ANIMATION)nSet);
		if (nSet == 2)
		{
			source_Pos = new Vector3(base.transform.position.x, base.transform.position.y + 2f, base.transform.position.z);
			lock_Pos = new Vector3(TargetPos.vec3.x, source_Pos.y, source_Pos.z);
			float distance = Vector2.Distance(source_Pos, lock_Pos);
			Vector3 vector = (source_Pos.xy() - lock_Pos.xy()).normalized;
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", source_Pos, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, -vector)), Array.Empty<object>()).SetEffect(distance, new Color(1f, 1f, 0f, 0.7f), new Color(1f, 0.54f, 0f), 1f, 3f);
		}
	}

	public void GetNextStatusByAI()
	{
		if (Activate)
		{
			if (AiState == AI_STATE.mob_003 && !StartCount)
			{
				FrameCount = GameLogicUpdateManager.GameFrame + rightTime * 20;
				StartCount = true;
			}
			if (_mainStatus == STAGE_MODEL_ANIMATION.Idle && Controller.Collisions.below)
			{
				UpdateRandomState();
			}
		}
	}

	public void ExploderEffectAndPos()
	{
		foreach (Transform item in ModelBody)
		{
			item.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.RenderEnemy;
		}
		if (ExplosionRoot != null)
		{
			if (_bDeadCallResult)
			{
				StartCoroutine(BossDieFlow(ExplosionRoot));
			}
			else
			{
				StartCoroutine(BossDieFlow(ExplosionRoot, "FX_BOSS_EXPLODE2", false, false));
			}
			return;
		}
		Vector3 targetTF = new Vector3(base.transform.position.x, base.transform.position.y + 1f, base.transform.position.z);
		if (_bDeadCallResult)
		{
			StartCoroutine(BossDieFlow(targetTF));
		}
		else
		{
			StartCoroutine(BossDieFlow(targetTF, "FX_BOSS_EXPLODE2", false, false));
		}
	}

	public void ExploderEffect()
	{
		foreach (Transform item in ModelBody)
		{
			item.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.RenderEnemy;
		}
		if (ExplosionRoot != null)
		{
			StartCoroutine(BossDieFlow(ExplosionRoot, "FX_BOSS_EXPLODE2", false, false));
			return;
		}
		Vector3 targetTF = new Vector3(base.transform.position.x, base.transform.position.y + 1f, base.transform.position.z);
		StartCoroutine(BossDieFlow(targetTF, "FX_BOSS_EXPLODE2", false, false));
	}

	public void CallIntro()
	{
		if (IntoBack != null)
		{
			IntoBack();
		}
		CanCallEvent = true;
	}

	public void RunAtkOn()
	{
		if (_mainStatus != STAGE_MODEL_ANIMATION.Dead)
		{
			Jump_Run_CollideBullet.UpdateBulletData(EnemyWeapons[5].BulletData);
			Jump_Run_CollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			Jump_Run_CollideBullet.Active(targetMask);
		}
	}

	public void RunStart()
	{
		if (base.transform.position.x > lock_Pos.x)
		{
			UpdateDirection(-1);
		}
		else
		{
			UpdateDirection(1);
		}
	}

	public void RunMoveUpdate()
	{
		if (_mainStatus == STAGE_MODEL_ANIMATION.Dead)
		{
			if (Jump_Run_CollideBullet.IsActivate)
			{
				Jump_Run_CollideBullet.IsDestroy = true;
			}
			return;
		}
		if (base.transform.position.x > lock_Pos.x)
		{
			Vector2 vector = new Vector2(base.transform.position.x, base.transform.position.y);
			if (RunFxName != "")
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(RunFxName, vector, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			}
			_velocity.x = -RunSpeed * 1000;
			if (base.transform.position.x <= lock_Pos.x + 1f)
			{
				if (AiState == AI_STATE.mob_003)
				{
					CallEventEnemyPoint(996);
					CallEventEnemyPoint(997);
				}
				SetStatus((STAGE_MODEL_ANIMATION)RndEndStatus);
				_velocity.x = 0;
				if (Jump_Run_CollideBullet.IsActivate)
				{
					Jump_Run_CollideBullet.IsDestroy = true;
				}
			}
			return;
		}
		Vector2 vector2 = new Vector2(base.transform.position.x, base.transform.position.y);
		if (RunFxName != "")
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(RunFxName, vector2, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
		}
		_velocity.x = RunSpeed * 1000;
		if (base.transform.position.x >= lock_Pos.x - 1f)
		{
			if (AiState == AI_STATE.mob_003)
			{
				CallEventEnemyPoint(998);
				CallEventEnemyPoint(999);
			}
			SetStatus((STAGE_MODEL_ANIMATION)RndEndStatus);
			_velocity.x = 0;
			if (Jump_Run_CollideBullet.IsActivate)
			{
				Jump_Run_CollideBullet.IsDestroy = true;
			}
		}
	}

	public void SetJumpSpeed()
	{
		_velocity.x = base.direction * JumpXSpeed * 1000;
		_velocity.y = JumpYSpeed * 1000;
	}

	public void MoveSpeedZero()
	{
		_velocity.x = 0;
		_velocity.y = 0;
	}

	public void JumpDownAtkOn()
	{
		if (JumpDownFxName != "")
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(JumpDownFxName, base.transform.position, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
		}
		Jump_Run_CollideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
		Jump_Run_CollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		Jump_Run_CollideBullet.Active(targetMask);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
	}

	public void JumpDownAtkOff()
	{
		if (Jump_Run_CollideBullet.IsActivate)
		{
			Jump_Run_CollideBullet.IsDestroy = true;
			if (AiState == AI_STATE.mob_003)
			{
				CallEventEnemyPoint(995);
			}
		}
	}

	public void Atk1On()
	{
		BoxCollider2D component = ClawCollideBullet.GetComponent<BoxCollider2D>();
		Vector3 vector = component.transform.TransformPoint(component.offset);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_maoh_000", new Vector3(vector.x, base.transform.position.y, vector.z), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
		ClawCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		ClawCollideBullet.Active(targetMask);
		PlayBossSE("BossSE", 55);
	}

	public void Atk1Off()
	{
		if (ClawCollideBullet.IsActivate)
		{
			ClawCollideBullet.IsDestroy = true;
		}
	}

	public void Atk2On()
	{
	}

	public void Atk2Off()
	{
	}

	public void Atk2Shot()
	{
		ShootBullet(3);
	}

	public void Atk3On()
	{
	}

	public void Atk3Off()
	{
	}

	public void Atk3Shot()
	{
		ShootBullet(4);
	}

	public override void LogicUpdate()
	{
		if (AiState == AI_STATE.mob_003 && GameLogicUpdateManager.GameFrame > FrameCount && StartCount && (int)Hp > 0)
		{
			if (callright)
			{
				CallEventEnemyPoint(994);
				callright = false;
				FrameCount = GameLogicUpdateManager.GameFrame + (leftTime - rightTime) * 20;
			}
			else
			{
				CallEventEnemyPoint(993);
				callright = true;
				FrameCount = GameLogicUpdateManager.GameFrame + (loopTime - leftTime + rightTime) * 20;
			}
		}
		if (!DebutUpdateDirection)
		{
			UpdateDirection();
			DebutUpdateDirection = true;
		}
		BaseUpdate();
		_mainStatus = (STAGE_MODEL_ANIMATION)_animator.GetInteger("NowStatus");
		UpdateGravity();
		_velocityExtra.z = 0;
		Controller.Move(_velocity * GameLogicUpdateManager.m_fFrameLen + _velocityExtra);
		distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_velocityExtra = VInt3.zero;
		if (bAlwaysStand)
		{
			_animator.SetBool("bBelow", true);
		}
		else
		{
			_animator.SetBool("bBelow", Controller.BelowInBypassRange);
		}
		_animator.SetInteger("nSpeedY", _velocity.y);
	}

	public void UpdateFunc()
	{
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		_velocityExtra += new VInt3(ModelTransform.localPosition);
		ModelTransform.localPosition = Vector3.zero;
	}

	private void NetGoToIdel()
	{
		StageUpdate.RegisterSendAndRun(sNetSerialID, 0);
		SetStatus(STAGE_MODEL_ANIMATION.IdleWaitNet);
	}

	private void UpdateRandomState()
	{
		fNowWaitTime += Time.deltaTime;
		if (fNowWaitTime < fNextWaitTime)
		{
			return;
		}
		fNowWaitTime = 0f;
		STAGE_MODEL_ANIMATION tempStatus = STAGE_MODEL_ANIMATION.Idle;
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			VInt3 zero = VInt3.zero;
			OrangeCharacter nearestPlayerByVintPos = StageUpdate.GetNearestPlayerByVintPos(Controller.LogicPosition, nMaxAtkRange);
			if (null != nearestPlayerByVintPos)
			{
				TargetPos = nearestPlayerByVintPos.Controller.LogicPosition;
				zero = Controller.LogicPosition - nearestPlayerByVintPos.Controller.LogicPosition;
				base.direction = ((zero.x <= 0) ? 1 : (-1));
				long sqrMagnitudeLong = zero.sqrMagnitudeLong;
				int num = OrangeBattleUtility.Random(0, 1000);
				for (int i = 0; i < listAIRD.Count; i++)
				{
					if (!((float)sqrMagnitudeLong < listAIRD[i].fRange * listAIRD[i].fRange * 1000000f))
					{
						continue;
					}
					for (int num2 = listAIRD[i].listAIData.Count - 1; num2 >= 0; num2--)
					{
						if (num > listAIRD[i].listAIData[num2].nRand)
						{
							tempStatus = (STAGE_MODEL_ANIMATION)listAIRD[i].listAIData[num2].nStatus;
							break;
						}
					}
					break;
				}
				if (tempStatus == STAGE_MODEL_ANIMATION.Run)
				{
					source_Pos = new Vector3(base.transform.position.x, base.transform.position.y + 2f, base.transform.position.z);
					lock_Pos = new Vector3(TargetPos.vec3.x, source_Pos.y, source_Pos.z);
					float distance = Vector2.Distance(source_Pos, lock_Pos);
					Vector3 vector = (source_Pos.xy() - lock_Pos.xy()).normalized;
					MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", source_Pos, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, -vector)), Array.Empty<object>()).SetEffect(distance, new Color(1f, 1f, 0f, 0.7f), new Color(1f, 0.54f, 0f), 1f, 3f);
				}
				UpdateFixAI(ref tempStatus);
			}
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
		}
		if (StageUpdate.bIsHost)
		{
			string sOther = "TargetPos," + TargetPos.x + "," + TargetPos.x + ",SelfPos," + Controller.LogicPosition.x + "," + Controller.LogicPosition.y + "," + Controller.LogicPosition.z;
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, (int)tempStatus, sOther);
		}
	}

	private void UpdateCollider()
	{
		STAGE_MODEL_ANIMATION mainStatus = _mainStatus;
		if (mainStatus == STAGE_MODEL_ANIMATION.Idle || (uint)(mainStatus - 7) <= 2u)
		{
			if (ClawCollideBullet.IsActivate)
			{
				ClawCollideBullet.IsDestroy = true;
			}
			if (Jump_Run_CollideBullet.IsActivate)
			{
				Jump_Run_CollideBullet.IsDestroy = true;
			}
		}
	}

	private void ShootBullet(int id)
	{
		Vector3 zero = Vector3.zero;
		zero = ShotPointFs[id - 2].position - ShotPointBs[id - 2].position;
		BulletBase bulletBase = BulletBase.TryShotBullet(EnemyWeapons[id].BulletData, ShotPointFs[id - 2], zero.normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
		_shootNum++;
		bulletBase.isBossBullet = true;
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (StageUpdate.gbIsNetGame)
		{
			if (TargetPos.x > Controller.LogicPosition.x)
			{
				base.direction = 1;
			}
			else
			{
				base.direction = -1;
			}
		}
		else if (StageUpdate.GetMainPlayerOC() != null && StageUpdate.GetMainPlayerOC().transform.position.x > _transform.position.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		PlayTurnSE("BossSE", 60);
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		base.transform.position = pos;
		if (ModelTransform == null)
		{
			Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
			ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg, base.UpdateHurtAction);
		if ((int)Hp > 0)
		{
			_characterMaterial.Hurt();
		}
		else if (_mainStatus != STAGE_MODEL_ANIMATION.Dead)
		{
			if (AiState == AI_STATE.mob_003)
			{
				StartCount = false;
			}
			OrangeBattleUtility.LockPlayer();
			ClawCollideBullet.BackToPool();
			Jump_Run_CollideBullet.BackToPool();
			_collideBullet.BackToPool();
			StageObjParam component = GetComponent<StageObjParam>();
			if (component == null || component.nEventID == 0)
			{
				StageUpdate.SlowStage();
				_velocity.x = 0;
				SetStatus(STAGE_MODEL_ANIMATION.Dead);
				if (Controller.Collisions.below)
				{
					_animator.Play("dead");
				}
				else
				{
					IgnoreGravity = true;
					_animator.Play("hurtdead");
				}
			}
			else
			{
				_velocity.x = 0;
				EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
				stageEventCall.nID = component.nEventID;
				component.nEventID = 0;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			}
		}
		return Hp;
	}

	public override void BossIntro(Action cb)
	{
		StartCoroutine(WaitChangeBossIntro(cb));
	}

	private IEnumerator WaitChangeBossIntro(Action cb)
	{
		while (_mainStatus == STAGE_MODEL_ANIMATION.Debut)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		SetStatus(STAGE_MODEL_ANIMATION.Debut);
		IntoBack = cb;
		Vector3 vector = new Vector3(base.transform.position.x, base.transform.position.y + 2f, 0f);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.left, 20f, 512);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector, Vector2.right, 20f, 512);
		if ((bool)raycastHit2D)
		{
			StageLiftPosX = raycastHit2D.point.x;
		}
		else
		{
			StageLiftPosX = base.transform.position.x - 20f;
		}
		if ((bool)raycastHit2D2)
		{
			StageRightPosX = raycastHit2D2.point.x;
		}
		else
		{
			StageRightPosX = base.transform.position.x + 20f;
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		ClawCollideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
		Jump_Run_CollideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
		AI_STATE aiState = AI_STATE.mob_001;
		if (EnemyData.s_AI != "null")
		{
			aiState = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
		}
		AiState = aiState;
		AI_STATE aiState2 = AiState;
		if ((uint)(aiState2 - 1) <= 1u)
		{
			_bDeadCallResult = false;
		}
		else
		{
			_bDeadCallResult = true;
		}
	}

	private void UpdateFixAI(ref STAGE_MODEL_ANIMATION tempStatus)
	{
		if (tempStatus != STAGE_MODEL_ANIMATION.Attack2 && tempStatus != STAGE_MODEL_ANIMATION.Attack3)
		{
			return;
		}
		if (_transform.position.x < StageLiftPosX + 1.5f || _transform.position.x > StageRightPosX - 1.5f)
		{
			fNowWaitTime = fNextWaitTime;
			tempStatus = STAGE_MODEL_ANIMATION.Idle;
		}
		if (tempStatus == STAGE_MODEL_ANIMATION.Attack3)
		{
			if (_transform.position.x > StageLiftPosX - 1f && _transform.position.x < StageLiftPosX + 7f && base.direction == -1)
			{
				fNowWaitTime = fNextWaitTime;
				tempStatus = STAGE_MODEL_ANIMATION.Idle;
			}
			if (_transform.position.x > StageLiftPosX + 4f && _transform.position.x < StageRightPosX + 6f && base.direction == 1)
			{
				fNowWaitTime = fNextWaitTime;
				tempStatus = STAGE_MODEL_ANIMATION.Idle;
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	private void CallEventEnemyPoint(int nID)
	{
		if (CanCallEvent)
		{
			EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
			stageEventCall.nID = nID;
			stageEventCall.tTransform = OrangeBattleUtility.CurrentCharacter.transform;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
		}
	}
}
