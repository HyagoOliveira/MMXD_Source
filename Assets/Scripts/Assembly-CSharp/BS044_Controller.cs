using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS044_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	[Serializable]
	private class MobPatternData
	{
		[Serializable]
		public class MobAction
		{
			public enum Action
			{
				Idle = 0,
				Move = 1,
				MoveLoop = 2,
				Skill = 3
			}

			public Action action;

			public Vector2 posStart;

			public Vector2 posEnd;

			public float moveSpeed;

			public bool isLeftThornActive;

			public bool isRightThornActive;

			public BS044_SubHeadController.Direction faceDirection;
		}

		public string patternName;

		public float patternTime;

		public MobAction[] actions;

		public MobPatternData()
		{
			patternTime = 6f;
			actions = new MobAction[3];
		}
	}

	[Serializable]
	private class FloatArray
	{
		[SerializeField]
		private float[] array;

		public float this[int index]
		{
			get
			{
				return array[index];
			}
		}

		public int Length
		{
			get
			{
				return array.Length;
			}
		}

		public FloatArray()
		{
			array = new float[1];
		}

		public FloatArray(int index)
		{
			array = new float[index];
		}
	}

	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Body_Shoot = 2,
		Body_Strafe = 3,
		Body_CallMob = 4,
		Head_Wind = 5,
		Head_Throw = 6,
		Head_CallMob = 7,
		Dead = 8,
		WaitNet = 9
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5
	}

	private enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_DEAD = 2,
		ANI_BODY_SHOOT_START = 3,
		ANI_BODY_SHOOT_LOOP = 4,
		ANI_BODY_SHOOT_END = 5,
		ANI_BODY_STRAFE_START = 6,
		ANI_BODY_STRAFE_LOOP = 7,
		ANI_BODY_STRAFE_END = 8,
		ANI_HEAD_IDLE = 9,
		ANI_HEAD_SKILL_START = 10,
		ANI_HEAD_SKILL_LOOP = 11,
		ANI_HEAD_SKILL_END = 12,
		ANIMATION_MAX_COUNT = 13
	}

	private const float DISSOLVE_TIME = 750f;

	[Header(" --- 槍手組件 --- ")]
	[SerializeField]
	private Transform bodyModelTransform;

	[SerializeField]
	private Transform bodyShootPointTransform;

	[SerializeField]
	private Transform bodyAimAxisTransform;

	[SerializeField]
	private CharacterMaterial bodyMaterial;

	[SerializeField]
	private Collider2D bodyCollider;

	[SerializeField]
	private CollideBullet bodyCollideBullet;

	[SerializeField]
	private CollideBullet strafeCollideBullet;

	[SerializeField]
	private CollideBullet shootCollideBullet;

	[Header(" --- 頭顱組件 --- ")]
	[SerializeField]
	private Animator headAnimator;

	[SerializeField]
	private Transform headModelTransform;

	[SerializeField]
	private Transform headShootPointTransform;

	[SerializeField]
	private CharacterMaterial headMaterial_1;

	[SerializeField]
	private CharacterMaterial headMaterial_2;

	[SerializeField]
	private CollideBullet headCollideBullet;

	[SerializeField]
	private Collider2D headCollider;

	[Header(" --- 特效 --- ")]
	[SerializeField]
	private Transform fxStrafePrepare;

	[SerializeField]
	private Transform fxStrafeAttack;

	[SerializeField]
	private Transform fxShootAttack;

	[SerializeField]
	private GameObject fxInhaleWind;

	[Header(" --- 設定參數 --- ")]
	[SerializeField]
	private int totalBodyShootTimes = 5;

	[SerializeField]
	private int totalHeadShootTimes = 10;

	[SerializeField]
	private float headShootIntervalTime = 0.2f;

	[SerializeField]
	private float headWindSpeed = 6f;

	[SerializeField]
	private float headInhaleDuration = 4f;

	[SerializeField]
	private float headInhaleMagnitude = 5f;

	[SerializeField]
	private MobPatternData gunShootPattern;

	[SerializeField]
	private MobPatternData headWindPattern;

	[SerializeField]
	private MobPatternData headThrowPattern;

	[SerializeField]
	private MobPatternData[] allMobData;

	[SerializeField]
	private FloatArray[] headThrowBulletPattern;

	private MainStatus mainStatus;

	private SubStatus subStatus;

	private MainStatus cacheStatus;

	private int[] animationHash;

	private float roomWidth;

	private float roomHeight;

	private Vector2 roomOriginPosition;

	private BS044_SubHeadController[] mobEnemyList = new BS044_SubHeadController[3];

	private MobPatternData currentMobData;

	private int mobCallPatternIndex;

	private int headThrowPatternIndex;

	private int shootCounter;

	private Vector3 lastLaserHitPosition;

	private OrangeCharacter currentStunPlayer;

	private bool isShowResultOnDead = true;

	private Action IntroCallback;

	private bool isShooting;

	private Vector3 positionOfBody;

	private Vector3 positionOfHead;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Start()
	{
		base.Start();
		IgnoreGravity = true;
		animationHash = new int[13];
		animationHash[0] = Animator.StringToHash("BS044@idle_loop");
		animationHash[1] = Animator.StringToHash("BS044@debut");
		animationHash[2] = Animator.StringToHash("BS044@dead");
		animationHash[3] = Animator.StringToHash("BS044@shoot_start");
		animationHash[4] = Animator.StringToHash("BS044@shoot_loop");
		animationHash[5] = Animator.StringToHash("BS044@shoot_end");
		animationHash[6] = Animator.StringToHash("BS044@strafe_start");
		animationHash[7] = Animator.StringToHash("BS044@strafe_loop");
		animationHash[8] = Animator.StringToHash("BS044@strafe_end");
		animationHash[9] = Animator.StringToHash("idle_loop");
		animationHash[10] = Animator.StringToHash("skill_start");
		animationHash[11] = Animator.StringToHash("skill_loop");
		animationHash[12] = Animator.StringToHash("skill_end");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_sigma_mode3_001", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_sigma_mode3_006");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_sigma_mode3_007", 10);
		headMaterial_1.Disappear(null, 0f);
		headMaterial_2.Disappear(null, 0f);
		SetStatus(MainStatus.Debut);
	}

	public void UpdateFunc()
	{
		if (Activate || mainStatus == MainStatus.Debut)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			UpdateStatusLogicPerFrame();
			UpdateWaitNetStatus();
		}
	}

	public override void LogicUpdate()
	{
		if (Activate || mainStatus == MainStatus.Debut)
		{
			base.LogicUpdate();
			UpdateStatusLogic();
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			Vector3 position = _transform.position;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(position, Vector2.left, 100f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(position, Vector2.right, 100f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(position, Vector2.up, 100f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
			RaycastHit2D raycastHit2D4 = Physics2D.Raycast(position, Vector2.down, 100f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
			roomWidth = Vector2.Distance(raycastHit2D.point, raycastHit2D2.point);
			roomHeight = Vector2.Distance(raycastHit2D3.point, raycastHit2D4.point);
			roomOriginPosition = new Vector2(raycastHit2D.point.x, raycastHit2D4.point.y);
			Vector2 roomPosition = GetRoomPosition(Vector2.one);
			positionOfBody = new Vector3(roomPosition.x - 1f, roomPosition.y - 3f, _transform.position.z);
			bodyModelTransform.position = positionOfBody;
			roomPosition = GetRoomPosition(Vector2.zero);
			positionOfHead = new Vector3(roomPosition.x + 2.5f, roomPosition.y, _transform.position.z);
			headModelTransform.position = positionOfHead;
			mobEnemyList = new BS044_SubHeadController[3];
			for (int i = 0; i < mobEnemyList.Length; i++)
			{
				MOB_TABLE tMOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[6 + i].BulletData.f_EFFECT_X];
				mobEnemyList[i] = (BS044_SubHeadController)StageUpdate.StageSpawnEnemyByMob(tMOB_TABLE, sNetSerialID + i);
			}
			fxStrafePrepare.gameObject.SetActive(false);
			fxStrafeAttack.gameObject.SetActive(false);
			fxShootAttack.gameObject.SetActive(false);
			bodyCollideBullet._transform.position = bodyModelTransform.position;
			bodyCollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			bodyCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			headCollideBullet._transform.position = headModelTransform.position;
			headCollideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			headCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			strafeCollideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
			strafeCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			shootCollideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
			shootCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		}
	}

	private void SetBodyActive(bool isActive)
	{
		if (isActive)
		{
			Controller.LogicPosition = new VInt3(positionOfBody);
			_transform.position = positionOfBody;
			ResetModelPosition();
			base.AimPoint = bodyCollider.transform.position - _transform.position;
			bodyMaterial.Appear(ActiveBodyCollider);
			bodyMaterial.isAllowHurtEffect = true;
			_characterMaterial = bodyMaterial;
		}
		else
		{
			base.AllowAutoAim = false;
			bodyMaterial.Disappear();
			bodyMaterial.isAllowHurtEffect = false;
			bodyCollider.enabled = false;
			bodyCollideBullet.BackToPool();
		}
	}

	private void SetHeadActive(bool isActive)
	{
		if (isActive)
		{
			Controller.LogicPosition = new VInt3(positionOfHead);
			_transform.position = positionOfHead;
			ResetModelPosition();
			base.AimPoint = headCollider.transform.position - _transform.position;
			headModelTransform.gameObject.SetActive(true);
			headMaterial_1.Appear(ActiveHeadCollider);
			headMaterial_2.Appear();
			headMaterial_1.isAllowHurtEffect = true;
			headMaterial_2.isAllowHurtEffect = true;
			_characterMaterial = headMaterial_1;
		}
		else
		{
			base.AllowAutoAim = false;
			headMaterial_1.Disappear();
			headMaterial_2.Disappear();
			headMaterial_1.isAllowHurtEffect = false;
			headMaterial_2.isAllowHurtEffect = false;
			headCollider.enabled = false;
			headCollideBullet.BackToPool();
		}
	}

	private void ActiveBodyCollider()
	{
		base.AllowAutoAim = true;
		bodyCollider.enabled = true;
		bodyCollideBullet.Active(targetMask);
	}

	private void ActiveHeadCollider()
	{
		base.AllowAutoAim = true;
		headCollider.enabled = true;
		headCollideBullet.Active(targetMask);
	}

	private void ResetModelPosition()
	{
		bodyModelTransform.position = positionOfBody;
		bodyCollideBullet._transform.position = positionOfBody;
		headModelTransform.position = positionOfHead;
		headCollideBullet._transform.position = positionOfHead;
	}

	public override void UpdateEnemyID(int _id)
	{
		base.UpdateEnemyID(_id);
		UpdateAIState();
		if (_enemyAutoAimSystem == null)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(30f);
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			isShowResultOnDead = false;
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		base.transform.position = pos;
	}

	public override void UpdateStatus(int _status, string _message, Callback _callback = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (_message != null && _message != "")
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(_message);
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
		SetStatus((MainStatus)_status);
	}

	protected void UpdateWaitNetStatus()
	{
		if (bWaitNetStatus && (!StageUpdate.gbIsNetGame || !StageUpdate.bIsHost))
		{
			bWaitNetStatus = false;
			SetStatus(cacheStatus);
		}
	}

	private void UpdateNextStatus()
	{
		if (StageUpdate.gbIsNetGame)
		{
			if (!bWaitNetStatus && StageUpdate.bIsHost)
			{
				cacheStatus = GetNextStatus(mainStatus);
				NetSyncData netSyncData = new NetSyncData();
				netSyncData.TargetPosX = TargetPos.x;
				netSyncData.TargetPosY = TargetPos.y;
				netSyncData.TargetPosZ = TargetPos.z;
				netSyncData.SelfPosX = Controller.LogicPosition.x;
				netSyncData.SelfPosY = Controller.LogicPosition.y;
				netSyncData.SelfPosZ = Controller.LogicPosition.z;
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)cacheStatus, JsonConvert.SerializeObject(netSyncData));
				mainStatus = MainStatus.WaitNet;
				bWaitNetStatus = true;
			}
		}
		else
		{
			if (bWaitNetStatus)
			{
				bWaitNetStatus = false;
			}
			cacheStatus = GetNextStatus(mainStatus);
			SetStatus(cacheStatus);
		}
	}

	private MainStatus GetNextStatus(MainStatus currentStatus)
	{
		switch (currentStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Body_Shoot:
		case MainStatus.Body_Strafe:
			return MainStatus.Body_CallMob;
		case MainStatus.Body_CallMob:
			return (MainStatus)OrangeBattleUtility.Random(5, 7);
		case MainStatus.Head_Wind:
		case MainStatus.Head_Throw:
			return MainStatus.Head_CallMob;
		case MainStatus.Head_CallMob:
			return (MainStatus)OrangeBattleUtility.Random(2, 4);
		default:
			return MainStatus.Idle;
		}
	}

	private void SetStatus(MainStatus _mainStatus, SubStatus _subStatus = SubStatus.Phase0)
	{
		mainStatus = _mainStatus;
		subStatus = _subStatus;
		switch (mainStatus)
		{
		case MainStatus.Debut:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				_animator.Play(animationHash[1], 0, 0f);
				break;
			case SubStatus.Phase3:
				SetBodyActive(false);
				SetHeadActive(false);
				break;
			}
			break;
		case MainStatus.Body_Shoot:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				SetBodyActive(true);
				shootCounter = 0;
				MobSummon(gunShootPattern);
				_animator.Play(animationHash[0], 0, 0f);
				break;
			case SubStatus.Phase1:
			{
				OrangeCharacter randomPlayer = OrangeBattleUtility.GetRandomPlayer();
				if (randomPlayer != null)
				{
					float value = Vector2.Angle(Vector2.down, randomPlayer._transform.position - bodyAimAxisTransform.position);
					_animator.SetFloat("Blend", value);
				}
				MobCommand();
				shootCounter++;
				_animator.Play(animationHash[3], 0, 0f);
				break;
			}
			case SubStatus.Phase2:
				PlaySE("BossSE03", "bs026_gisig01");
				fxShootAttack.gameObject.SetActive(true);
				fxShootAttack.position = bodyShootPointTransform.position;
				fxShootAttack.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, bodyShootPointTransform.forward));
				_animator.Play(animationHash[4], 0, 0f);
				break;
			case SubStatus.Phase3:
				fxShootAttack.gameObject.SetActive(false);
				shootCollideBullet.BackToPool();
				_animator.Play(animationHash[5], 0, 0f);
				break;
			case SubStatus.Phase4:
				SetBodyActive(false);
				MobHide();
				break;
			}
			break;
		case MainStatus.Body_Strafe:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				SetBodyActive(true);
				MobSummon(gunShootPattern);
				_animator.Play(animationHash[0], 0, 0f);
				break;
			case SubStatus.Phase1:
				PlayBossSE("BossSE03", "bs026_gisig02");
				fxStrafePrepare.gameObject.SetActive(true);
				MobCommand();
				_animator.Play(animationHash[6], 0, 0f);
				break;
			case SubStatus.Phase2:
				isShooting = false;
				_animator.Play(animationHash[8], 0, 0f);
				break;
			case SubStatus.Phase3:
				fxStrafeAttack.gameObject.SetActive(false);
				strafeCollideBullet.BackToPool();
				break;
			case SubStatus.Phase4:
				SetBodyActive(false);
				MobHide();
				break;
			}
			break;
		case MainStatus.Head_Wind:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				SetHeadActive(true);
				MobSummon(headWindPattern);
				headAnimator.Play(animationHash[9], 0, 0f);
				break;
			case SubStatus.Phase1:
				MobCommand();
				headAnimator.Play(animationHash[11], 0, 0f);
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.right * headWindSpeed;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_sigma_mode3_006", GetRoomPosition(new Vector2(0.5f, 0.5f)), Quaternion.identity, Array.Empty<object>());
				PlaySE("BossSE03", "bs026_gisig08");
				break;
			case SubStatus.Phase2:
				headAnimator.Play(animationHash[12], 0, 0f);
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.zero;
				break;
			case SubStatus.Phase3:
				SetHeadActive(false);
				MobHide();
				break;
			}
			break;
		case MainStatus.Head_Throw:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				SetHeadActive(true);
				MobSummon(headThrowPattern);
				shootCounter = 0;
				headAnimator.Play(animationHash[9], 0, 0f);
				break;
			case SubStatus.Phase1:
				MobCommand();
				fxInhaleWind.SetActive(true);
				headAnimator.Play(animationHash[11], 0, 0f);
				PlaySE("BossSE03", "bs026_gisig06");
				break;
			case SubStatus.Phase2:
				if (currentStunPlayer != null)
				{
					currentStunPlayer.SetStun(false);
					currentStunPlayer = null;
				}
				headAnimator.Play(animationHash[11], 0, 0f);
				break;
			case SubStatus.Phase3:
				headThrowPatternIndex++;
				if (headThrowPatternIndex >= headThrowBulletPattern.Length)
				{
					headThrowPatternIndex = 0;
				}
				headAnimator.Play(animationHash[12], 0, 0f);
				break;
			case SubStatus.Phase4:
				SetHeadActive(false);
				MobHide();
				break;
			}
			break;
		case MainStatus.Body_CallMob:
		case MainStatus.Head_CallMob:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (allMobData.Length == 0)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
				}
				MobSummon(allMobData[mobCallPatternIndex]);
				break;
			case SubStatus.Phase1:
				MobCommand();
				break;
			case SubStatus.Phase2:
				MobHide();
				break;
			case SubStatus.Phase3:
				MobRecall();
				mobCallPatternIndex++;
				if (mobCallPatternIndex >= allMobData.Length)
				{
					mobCallPatternIndex = 0;
				}
				break;
			}
			break;
		}
		AiTimer.TimerStart();
	}

	private void UpdateStatusLogic()
	{
		float normalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (mainStatus)
		{
		case MainStatus.Idle:
			UpdateNextStatus();
			_velocity.x = 0;
			break;
		case MainStatus.Debut:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if ((double)normalizedTime >= 1.0)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_introReady)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
					if (IntroCallback != null)
					{
						IntroCallback();
					}
				}
				break;
			case SubStatus.Phase2:
				if (Activate)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((float)AiTimer.GetMillisecond() > 750f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Body_Shoot:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (AiTimer.GetMillisecond() > 1000)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (normalizedTime >= 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (AiTimer.GetMillisecond() > 1500)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
				}
				else if (!shootCollideBullet.IsActivate && AiTimer.GetMillisecond() > 500)
				{
					shootCollideBullet._transform.position = bodyShootPointTransform.position;
					shootCollideBullet._transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, bodyShootPointTransform.forward));
					shootCollideBullet.Active(targetMask);
				}
				break;
			case SubStatus.Phase3:
				if (normalizedTime >= 1f)
				{
					if (shootCounter < totalBodyShootTimes)
					{
						SetStatus(mainStatus, SubStatus.Phase1);
					}
					else
					{
						SetStatus(mainStatus, SubStatus.Phase4);
					}
				}
				break;
			case SubStatus.Phase4:
				if ((float)AiTimer.GetMillisecond() > 750f)
				{
					MobRecall();
					UpdateNextStatus();
				}
				break;
			}
			break;
		case MainStatus.Body_Strafe:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (AiTimer.GetMillisecond() > 1000)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (normalizedTime >= 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (normalizedTime >= 0.5f)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
				}
				else if (normalizedTime >= 0.1f && !isShooting)
				{
					isShooting = true;
					strafeCollideBullet.Active(targetMask);
					fxStrafeAttack.gameObject.SetActive(true);
					fxStrafeAttack.position = bodyShootPointTransform.position;
					fxStrafeAttack.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, bodyShootPointTransform.forward));
				}
				break;
			case SubStatus.Phase3:
				if (normalizedTime >= 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if ((float)AiTimer.GetMillisecond() > 750f)
				{
					MobRecall();
					UpdateNextStatus();
				}
				break;
			}
			break;
		case MainStatus.Head_Wind:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (AiTimer.GetMillisecond() > 1000)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (AiTimer.GetMillisecond() > 5000)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (AiTimer.GetMillisecond() > 2000)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((float)AiTimer.GetMillisecond() > 750f)
				{
					MobRecall();
					UpdateNextStatus();
				}
				break;
			}
			break;
		case MainStatus.Head_Throw:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (AiTimer.GetMillisecond() > 1000)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				if ((float)AiTimer.GetMillisecond() > headInhaleDuration * 1000f)
				{
					fxInhaleWind.SetActive(false);
					OrangeBattleUtility.GlobalVelocityExtra = VInt3.zero;
					SetStatus(mainStatus, SubStatus.Phase2);
					break;
				}
				OrangeCharacter closetPlayer = _enemyAutoAimSystem.GetClosetPlayer();
				if (!(closetPlayer != null))
				{
					break;
				}
				if (Vector2.Distance(closetPlayer._transform.position, headModelTransform.position) > 1f)
				{
					if (closetPlayer._transform.position.x > headModelTransform.position.x)
					{
						OrangeBattleUtility.GlobalVelocityExtra = VInt3.right * (0f - headWindSpeed);
					}
					else
					{
						OrangeBattleUtility.GlobalVelocityExtra = VInt3.right * headWindSpeed;
					}
					break;
				}
				currentStunPlayer = closetPlayer;
				if (!currentStunPlayer.IsStun)
				{
					currentStunPlayer.SetStun(true);
				}
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.zero;
				break;
			}
			case SubStatus.Phase2:
				if ((float)AiTimer.GetMillisecond() > (float)shootCounter * headShootIntervalTime * 1000f)
				{
					if (shootCounter < headThrowBulletPattern[headThrowPatternIndex].Length)
					{
						int num = OrangeBattleUtility.Random(4, 6);
						BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, headShootPointTransform.position, GetRoomPosition(new Vector2(headThrowBulletPattern[headThrowPatternIndex][shootCounter], 0f)), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_sigma_mode3_007", headShootPointTransform.position, headShootPointTransform.rotation, Array.Empty<object>());
					}
					else
					{
						SetStatus(mainStatus, SubStatus.Phase3);
					}
					shootCounter++;
				}
				break;
			case SubStatus.Phase3:
				if (AiTimer.GetMillisecond() > 2000)
				{
					SetStatus(mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if ((float)AiTimer.GetMillisecond() > 750f)
				{
					MobRecall();
					UpdateNextStatus();
				}
				break;
			}
			break;
		case MainStatus.Body_CallMob:
		case MainStatus.Head_CallMob:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if ((float)AiTimer.GetMillisecond() > 750f)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((float)AiTimer.GetMillisecond() > currentMobData.patternTime * 1000f)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((float)AiTimer.GetMillisecond() > 750f)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				UpdateNextStatus();
				break;
			}
			break;
		}
		if (currentStunPlayer != null && (mainStatus != MainStatus.Head_Throw || subStatus != SubStatus.Phase1))
		{
			currentStunPlayer.SetStun(false);
			currentStunPlayer = null;
		}
	}

	private void UpdateStatusLogicPerFrame()
	{
		MainStatus mainStatus = this.mainStatus;
		if (mainStatus != MainStatus.Body_Strafe || subStatus != SubStatus.Phase2 || !isShooting)
		{
			return;
		}
		fxStrafeAttack.position = bodyShootPointTransform.position;
		fxStrafeAttack.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, bodyShootPointTransform.forward));
		RaycastHit2D raycastHit2D = Physics2D.Raycast(bodyShootPointTransform.position, bodyShootPointTransform.forward, 100f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		if (!(raycastHit2D.collider != null))
		{
			return;
		}
		float num = Vector2.Distance(bodyShootPointTransform.position, raycastHit2D.point);
		fxStrafeAttack.transform.localScale = new Vector3(num, 1f, 1f);
		strafeCollideBullet._transform.position = bodyShootPointTransform.position;
		strafeCollideBullet._transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, bodyShootPointTransform.forward));
		BoxCollider2D boxCollider2D = (BoxCollider2D)strafeCollideBullet.GetHitCollider();
		if (boxCollider2D != null)
		{
			boxCollider2D.offset = new Vector2(num * 0.5f, 0f);
			boxCollider2D.size = new Vector2(num, 0.2f);
		}
		if (Vector2.Distance(lastLaserHitPosition, raycastHit2D.point) > 1f)
		{
			if (Vector2.Angle(Vector2.up, raycastHit2D.normal) > 45f)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_sigma_mode3_001", raycastHit2D.point, Quaternion.identity, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_sigma_mode3_001", raycastHit2D.point, Quaternion.identity, Array.Empty<object>());
			}
			lastLaserHitPosition = raycastHit2D.point;
		}
	}

	public override void BossIntro(Action _callback)
	{
		_introReady = true;
		SetStatus(mainStatus, SubStatus.Phase1);
		IntroCallback = _callback;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (mainStatus != MainStatus.Dead)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)bodyCollideBullet)
			{
				bodyCollideBullet.BackToPool();
			}
			if ((bool)strafeCollideBullet)
			{
				strafeCollideBullet.BackToPool();
			}
			if ((bool)shootCollideBullet)
			{
				shootCollideBullet.BackToPool();
			}
			if ((bool)headCollideBullet)
			{
				headCollideBullet.BackToPool();
			}
			_velocity.x = 0;
			OrangeBattleUtility.GlobalVelocityExtra = VInt3.zero;
			MobRecall();
			fxStrafePrepare.gameObject.SetActive(false);
			fxStrafeAttack.gameObject.SetActive(false);
			fxShootAttack.gameObject.SetActive(false);
			fxInhaleWind.gameObject.SetActive(false);
			Transform transForm = _transform;
			switch (mainStatus)
			{
			case MainStatus.Body_Shoot:
			case MainStatus.Body_Strafe:
				transForm = bodyModelTransform;
				break;
			case MainStatus.Head_Wind:
			case MainStatus.Head_Throw:
				transForm = headModelTransform;
				break;
			}
			if (isShowResultOnDead)
			{
				StartCoroutine(BossDieFlow(transForm));
			}
			else
			{
				StartCoroutine(BossDieFlow(transForm, "FX_BOSS_EXPLODE2", false, false));
			}
			StageUpdate.SlowStage();
			SetStatus(MainStatus.Dead);
		}
	}

	private void MobSummon(MobPatternData pattern)
	{
		currentMobData = pattern;
		for (int i = 0; i < mobEnemyList.Length; i++)
		{
			MOB_TABLE mOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[6 + i].BulletData.f_EFFECT_X];
			mobEnemyList[i] = (BS044_SubHeadController)StageUpdate.StageSpawnEnemyByMob(mOB_TABLE, sNetSerialID + i);
			if (mobEnemyList[i] != null)
			{
				mobEnemyList[i].UpdateEnemyID(mOB_TABLE.n_ID);
				mobEnemyList[i].SetPositionAndRotation(GetRoomPosition(currentMobData.actions[i].posStart), false);
				mobEnemyList[i].SetThornActive(currentMobData.actions[i].isLeftThornActive, currentMobData.actions[i].isRightThornActive);
				mobEnemyList[i].SetActive(true);
			}
		}
	}

	private void MobCommand()
	{
		if (currentMobData == null)
		{
			return;
		}
		for (int i = 0; i < mobEnemyList.Length; i++)
		{
			if (!(mobEnemyList[i] == null))
			{
				switch (currentMobData.actions[i].action)
				{
				case MobPatternData.MobAction.Action.Move:
					mobEnemyList[i].StartMove(GetRoomPosition(currentMobData.actions[i].posStart), GetRoomPosition(currentMobData.actions[i].posEnd), currentMobData.actions[i].moveSpeed);
					break;
				case MobPatternData.MobAction.Action.MoveLoop:
					mobEnemyList[i].StartMove(GetRoomPosition(currentMobData.actions[i].posStart), GetRoomPosition(currentMobData.actions[i].posEnd), currentMobData.actions[i].moveSpeed, true);
					break;
				case MobPatternData.MobAction.Action.Skill:
					mobEnemyList[i].StartUseSkill();
					break;
				}
			}
		}
	}

	private void MobHide()
	{
		if (currentMobData == null)
		{
			return;
		}
		for (int i = 0; i < mobEnemyList.Length; i++)
		{
			if (mobEnemyList[i] != null && mobEnemyList[i].InGame)
			{
				mobEnemyList[i].StopAndHide();
			}
		}
	}

	private void MobRecall()
	{
		if (currentMobData == null)
		{
			return;
		}
		for (int i = 0; i < mobEnemyList.Length; i++)
		{
			if (mobEnemyList[i] != null && mobEnemyList[i].InGame)
			{
				mobEnemyList[i].SetActive(false);
				mobEnemyList[i].BackToPool();
			}
		}
	}

	private Vector2 GetRoomPosition(Vector2 position)
	{
		Vector2 result = roomOriginPosition;
		result.x += roomWidth * position.x;
		result.y += roomHeight * position.y;
		return result;
	}
}
