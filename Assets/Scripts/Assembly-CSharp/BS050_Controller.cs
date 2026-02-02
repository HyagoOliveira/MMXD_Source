using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS050_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Die = 4,
		IdleChip = 5
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		Phase6 = 6,
		MAX_SUBSTATUS = 7
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT_LOOP = 1,
		ANI_DEBUT_END = 2,
		ANI_Skill0_START = 3,
		ANI_Skill0_END = 4,
		ANI_Skill1_START = 5,
		ANI_Skill1_LOOP = 6,
		ANI_Skill1_END = 7,
		ANI_HURT = 8,
		ANI_DEAD = 9,
		MAX_ANIMATION_ID = 10
	}

	private enum EyeStatus
	{
		Open = 0,
		Opening = 1,
		Close = 2,
		Closing = 3
	}

	private enum EyeAnimationID
	{
		ANI_OPEN = 0,
		ANI_OPENING = 1,
		ANI_CLOSE = 2,
		ANI_CLOSING = 3,
		MAX_EYE_ID = 4
	}

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus _mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus _subStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private EyeStatus _eyestatus = EyeStatus.Close;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private AnimationID _currentAnimationId;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private float _currentFrame;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private float _currentEyeFrame;

	private int nDeadCount;

	private int[] _animationHash;

	private int[] _eyeanimationHash;

	private int[] DefaultSkillCard = new int[4] { 0, 0, 1, 2 };

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[SerializeField]
	private SkinnedMeshRenderer[] Body1Mesh = new SkinnedMeshRenderer[2];

	[SerializeField]
	private SkinnedMeshRenderer[] Body2Mesh = new SkinnedMeshRenderer[2];

	[SerializeField]
	private Transform SeparatedTransform1;

	[SerializeField]
	private Transform SeparatedTransform2;

	[SerializeField]
	private SkinnedMeshRenderer[] Model1Mesh = new SkinnedMeshRenderer[21];

	[SerializeField]
	private SkinnedMeshRenderer[] Model2Mesh = new SkinnedMeshRenderer[21];

	[SerializeField]
	private float Spacing = 5f;

	private Transform ModelTransform2;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private Animator _animator2;

	private CollideBullet _collideBullet2;

	private CollideBulletHitSelf _recoverBullet1;

	private CollideBulletHitSelf _recoverBullet2;

	private List<BS050_CollideBullet> PartCollide1 = new List<BS050_CollideBullet>();

	private List<BS050_CollideBullet> PartCollide2 = new List<BS050_CollideBullet>();

	[SerializeField]
	private float SplitTime = 1f;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private int SplitFrame;

	private int HidePartNum;

	private int ShowPartNum;

	private bool _isEyeOpen;

	[SerializeField]
	private GameObject EnemyCollide1;

	[SerializeField]
	private GameObject EnemyCollide2;

	private int ShootTimes;

	private int Skill1Rounds;

	private float ShootFrame;

	private bool HasShoot;

	private Transform ShootPos;

	private Vector3 EndPos;

	[SerializeField]
	private Transform[] EyeShootPos = new Transform[2];

	[SerializeField]
	private Transform[] ShootPoint1 = new Transform[21];

	[SerializeField]
	private Transform[] ShootPoint2 = new Transform[21];

	[SerializeField]
	private int[] ShootOrder = new int[21]
	{
		7, 3, 12, 17, 4, 8, 13, 18, 0, 14,
		5, 1, 9, 19, 15, 2, 10, 6, 20, 11,
		16
	};

	[SerializeField]
	private float IdleWaitTime = 1f;

	private int IdleWaitFrame;

	private MainStatus lastmove = MainStatus.Skill0;

	private bool CanSummon;

	private bool DeadCallResult;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private bool IsChipInfoAnim;

	private int HidePartOrder
	{
		get
		{
			return ShootOrder[20 - HidePartNum];
		}
	}

	private int ShowPartOrder
	{
		get
		{
			return ShootOrder[20 - ShowPartNum];
		}
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected void ChangeDebugMode()
	{
		DebugMode = !DebugMode;
	}

	protected void ChangeSetSkill(object[] param)
	{
		string text = param[0] as string;
		if (!(text == string.Empty))
		{
			switch (text)
			{
			case "Idle":
				NextSkill = MainStatus.Idle;
				break;
			case "Skill0":
				NextSkill = MainStatus.Skill0;
				break;
			case "Skill1":
				NextSkill = MainStatus.Skill1;
				break;
			}
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[10];
		_animationHash[0] = Animator.StringToHash("BS050@idle_loop");
		_animationHash[3] = Animator.StringToHash("BS050@skill_01_start");
		_animationHash[4] = Animator.StringToHash("BS050@skill_01_end");
		_animationHash[5] = Animator.StringToHash("BS050@skill_02_step1_start");
		_animationHash[6] = Animator.StringToHash("BS050@skill_02_step2_start");
		_animationHash[7] = Animator.StringToHash("BS050@skill_02_end");
		_animationHash[8] = Animator.StringToHash("BS050@hurt_loop");
		_animationHash[9] = Animator.StringToHash("BS050@dead");
		_eyeanimationHash = new int[4];
		_eyeanimationHash[0] = Animator.StringToHash("BS050@eye_open_loop");
		_eyeanimationHash[1] = Animator.StringToHash("BS050@eye_open_start");
		_eyeanimationHash[2] = Animator.StringToHash("BS050@eye_close_loop");
		_eyeanimationHash[3] = Animator.StringToHash("BS050@eye_close_start");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model1", true);
		ModelTransform2 = OrangeBattleUtility.FindChildRecursive(ref childs, "model2", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_animator2 = ModelTransform2.GetComponent<Animator>();
		if (SeparatedTransform1 == null)
		{
			SeparatedTransform1 = OrangeBattleUtility.FindChildRecursive(ref childs, "SeparatedMesh1", true);
		}
		if (SeparatedTransform2 == null)
		{
			SeparatedTransform2 = OrangeBattleUtility.FindChildRecursive(ref childs, "SeparatedMesh2", true);
		}
		Model1Mesh = SeparatedTransform1.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		Model2Mesh = SeparatedTransform2.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		ShootPoint1 = OrangeBattleUtility.FindAllChildRecursive(SeparatedTransform1, "Shoot");
		ShootPoint2 = OrangeBattleUtility.FindAllChildRecursive(SeparatedTransform2, "Shoot");
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collide1", true).gameObject.AddOrGetComponent<CollideBullet>();
		_collideBullet2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Collide2", true).gameObject.AddOrGetComponent<CollideBullet>();
		_recoverBullet1 = OrangeBattleUtility.FindChildRecursive(ref childs, "RecoverCollide1", true).gameObject.AddOrGetComponent<CollideBulletHitSelf>();
		_recoverBullet2 = OrangeBattleUtility.FindChildRecursive(ref childs, "RecoverCollide2", true).gameObject.AddOrGetComponent<CollideBulletHitSelf>();
		for (int i = 0; i < ShootPoint1.Length; i++)
		{
			PartCollide1.Add(ShootPoint1[i].gameObject.AddOrGetComponent<BS050_CollideBullet>());
			PartCollide2.Add(ShootPoint2[i].gameObject.AddOrGetComponent<BS050_CollideBullet>());
		}
		if (EnemyCollide1 == null)
		{
			EnemyCollide1 = OrangeBattleUtility.FindChildRecursive(ref childs, "EnemyCollide1", true).gameObject;
		}
		if (EnemyCollide2 == null)
		{
			EnemyCollide2 = OrangeBattleUtility.FindChildRecursive(ref childs, "EnemyCollide2", true).gameObject;
		}
		if (Body1Mesh[0] == null)
		{
			Body1Mesh[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS050_HasEyeMesh1_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (Body1Mesh[1] == null)
		{
			Body1Mesh[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS050_NoEyeMesh1_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (Body2Mesh[0] == null)
		{
			Body2Mesh[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS050_HasEyeMesh2_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (Body2Mesh[1] == null)
		{
			Body2Mesh[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS050_NoEyeMesh2_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (EyeShootPos[0] == null)
		{
			EyeShootPos[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "EyeBone_Nub1");
		}
		if (EyeShootPos[1] == null)
		{
			EyeShootPos[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "EyeBone_Nub2");
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimPoint = Vector3.zero;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		FallDownSE = new string[2] { "BattleSE", "bt_ridearmor02" };
		AiTimer.TimerStart();
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (smsg != null && smsg != "")
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
		lastmove = (MainStatus)nSet;
		SetStatus((MainStatus)nSet);
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (TargetPos.x > Controller.LogicPosition.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)(-base.direction));
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				EnemyCollide1.SetActive(false);
				EnemyCollide2.SetActive(false);
				break;
			case SubStatus.Phase1:
				SeparatedTransform2.localPosition = Vector3.left * (Spacing + 10f);
				base.direction *= -1;
				_isEyeOpen = false;
				break;
			case SubStatus.Phase2:
				SplitFrame = GameLogicUpdateManager.GameFrame + (int)(SplitTime * 20f);
				break;
			}
			break;
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ChangeModel(true, base.direction);
				ChangeEyeModel(true, base.direction);
				_isEyeOpen = false;
				break;
			case SubStatus.Phase1:
				SplitFrame = GameLogicUpdateManager.GameFrame + (int)(SplitTime * 20f);
				break;
			case SubStatus.Phase2:
				_isEyeOpen = true;
				ChangeModel(true, base.direction);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE03", "bs027_yellow06");
				ChangeModel(true, base.direction);
				ChangeEyeModel(true, base.direction);
				ShootTimes = 2;
				break;
			case SubStatus.Phase1:
				ShootFrame = 0.2f;
				HasShoot = false;
				ShootPos = ((base.direction == 1) ? EyeShootPos[1] : EyeShootPos[0]);
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE03", "bs027_yellow11");
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				nDeadCount = 0;
				SetStatus(MainStatus.Die, SubStatus.Phase1);
				break;
			case SubStatus.Phase2:
				if (DeadCallResult)
				{
					StartCoroutine(BossDieFlow(GetTargetPoint()));
				}
				else
				{
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
				}
				break;
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Debut:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase1)
			{
				_currentAnimationId = AnimationID.ANI_Skill0_START;
			}
			break;
		}
		case MainStatus.Idle:
		case MainStatus.IdleChip:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill0_START;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill0_END;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill1_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill1_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill1_END;
				break;
			}
			break;
		case MainStatus.Die:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase1)
			{
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			}
			return;
		}
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
		_animator2.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		if (_mainStatus == MainStatus.Idle)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				if (lastmove == MainStatus.Skill1 && Skill1Rounds >= 3)
				{
					Skill1Rounds = 0;
					mainStatus = MainStatus.Skill0;
				}
				else
				{
					Skill1Rounds++;
					mainStatus = MainStatus.Skill1;
				}
			}
			else
			{
				mainStatus = MainStatus.Skill0;
			}
		}
		if (DebugMode)
		{
			mainStatus = NextSkill;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
	}

	public override void LogicUpdate()
	{
		if (_mainStatus == MainStatus.Debut)
		{
			BaseUpdate();
			UpdateGravity();
			Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (IntroCallBack != null)
				{
					IntroCallBack();
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && _eyestatus == EyeStatus.Close && _unlockReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (HidePartNum >= Model1Mesh.Length && ShowPartNum >= HidePartNum)
				{
					base.direction *= -1;
					ShowPartNum = (HidePartNum = 0);
					SeparatedTransform2.localPosition = Vector3.left * Spacing;
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				else if (GameLogicUpdateManager.GameFrame > SplitFrame && HidePartNum < Model1Mesh.Length)
				{
					ShootAndHidePart();
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus && IdleWaitFrame < GameLogicUpdateManager.GameFrame)
			{
				UpdateRandomState();
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f && _eyestatus == EyeStatus.Close)
				{
					if (CanSummon)
					{
						MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
					}
					ChangeModel(false, base.direction);
					IsInvincible = true;
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (HidePartNum >= Model1Mesh.Length && ShowPartNum >= HidePartNum)
				{
					base.direction *= -1;
					ShowPartNum = (HidePartNum = 0);
					IsInvincible = false;
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				else if (GameLogicUpdateManager.GameFrame > SplitFrame && HidePartNum < Model1Mesh.Length)
				{
					ShootAndHidePart();
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f && _eyestatus == EyeStatus.Open)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					if (ShootTimes > 0)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase1);
						break;
					}
					PlaySE("BossSE03", "bs027_yellow05");
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				else if (_currentFrame > ShootFrame && !HasShoot)
				{
					EndPos = ShootPos.position + Vector3.right * 5f * base.direction + Vector3.down * 2f;
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					if ((bool)Target)
					{
						EndPos = Target.Controller.GetRealCenterPos() - ShootPos.position;
					}
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos, EndPos, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					ShootTimes--;
					HasShoot = true;
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Die:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase1 && _currentFrame > 0.36f)
			{
				if (nDeadCount > 10)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				else
				{
					nDeadCount++;
				}
			}
			break;
		}
		}
		UpdateEyeCtrl();
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	private void ShootAndHidePart()
	{
		if (base.direction == -1)
		{
			Model1Mesh[HidePartOrder].enabled = false;
			ShootPos = ShootPoint1[HidePartOrder];
			EndPos = ShootPoint2[HidePartOrder].position;
			if ((bool)PartCollide1[HidePartOrder])
			{
				PartCollide1[HidePartOrder].HitCallback = null;
				PartCollide1[HidePartOrder].BackToPool();
			}
		}
		else
		{
			Model2Mesh[HidePartOrder].enabled = false;
			ShootPos = ShootPoint2[HidePartOrder];
			EndPos = ShootPoint1[HidePartOrder].position;
			if ((bool)PartCollide2[HidePartOrder])
			{
				PartCollide2[HidePartOrder].HitCallback = null;
				PartCollide2[HidePartOrder].BackToPool();
			}
		}
		BasicBullet obj = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BasicBullet;
		obj.BackCallback = ShowPart;
		obj.FreeDISTANCE = Vector3.Distance(ShootPos.position, EndPos) + 0.4f;
		HidePartNum++;
	}

	private void ShowPart(object obj = null)
	{
		if (base.direction == -1)
		{
			Model2Mesh[ShowPartOrder].enabled = true;
			PartCollide2[ShowPartOrder].HitCallback = Part2HitCallBack;
			PartCollide2[ShowPartOrder].Active(targetMask);
		}
		else
		{
			Model1Mesh[ShowPartOrder].enabled = true;
			PartCollide1[ShowPartOrder].HitCallback = Part1HitCallBack;
			PartCollide1[ShowPartOrder].Active(targetMask);
		}
		PlaySE("BossSE03", "bs027_yellow02");
		ShowPartNum++;
	}

	public override void SetActive(bool isActive)
	{
		IgnoreGravity = true;
		base.SetActive(isActive);
		if (isActive)
		{
			ModelTransform.localPosition = Vector3.zero;
			ModelTransform2.localPosition = Vector3.left * Spacing;
			SeparatedTransform1.localPosition = Vector3.zero;
			SeparatedTransform2.localPosition = Vector3.left * Spacing;
			_isEyeOpen = false;
			for (int i = 0; i < Model1Mesh.Length; i++)
			{
				Model1Mesh[i].enabled = false;
				Model2Mesh[i].enabled = false;
			}
			for (int j = 0; j < Body1Mesh.Length; j++)
			{
				Body1Mesh[j].enabled = false;
				Body2Mesh[j].enabled = false;
			}
			lastmove = MainStatus.Skill0;
			for (int k = 0; k < PartCollide1.Count; k++)
			{
				if ((bool)PartCollide1[k])
				{
					PartCollide1[k].UpdateBulletData(EnemyWeapons[3].BulletData);
					PartCollide1[k].SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				}
				if ((bool)PartCollide2[k])
				{
					PartCollide2[k].UpdateBulletData(EnemyWeapons[3].BulletData);
					PartCollide2[k].SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				}
			}
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet2.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet2.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_recoverBullet1.UpdateBulletData(EnemyWeapons[4].BulletData, "", base.gameObject.GetInstanceID());
			_recoverBullet1.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_recoverBullet1.HitCallback = HealCollideHitCallBack;
			_recoverBullet2.UpdateBulletData(EnemyWeapons[4].BulletData, "", base.gameObject.GetInstanceID());
			_recoverBullet2.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_recoverBullet2.HitCallback = HealCollideHitCallBack;
			SetStatus(MainStatus.Debut);
			return;
		}
		_recoverBullet1.BackToPool();
		_recoverBullet2.BackToPool();
		_collideBullet.BackToPool();
		_collideBullet2.BackToPool();
		for (int l = 0; l < PartCollide1.Count; l++)
		{
			if ((bool)PartCollide1[l])
			{
				PartCollide1[l].BackToPool();
			}
			if ((bool)PartCollide2[l])
			{
				PartCollide2[l].BackToPool();
			}
		}
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
		_transform.position = pos;
	}

	public override void BossIntro(Action cb)
	{
		IntroCallBack = cb;
		_introReady = true;
	}

	public override void Unlock()
	{
		_unlockReady = true;
		base.AllowAutoAim = true;
		if ((int)Hp > 0)
		{
			SetColliderEnable(true);
		}
		if (InGame)
		{
			Activate = true;
		}
	}

	public override void SetChipInfoAnim()
	{
		SetStatus(MainStatus.IdleChip);
		IsChipInfoAnim = true;
		UpdateAnimation();
		for (int i = 0; i < Model1Mesh.Length; i++)
		{
			Model1Mesh[i].enabled = false;
			Model2Mesh[i].enabled = false;
		}
		for (int j = 0; j < Body1Mesh.Length; j++)
		{
			Body1Mesh[j].enabled = false;
		}
		Body2Mesh[0].enabled = true;
		Body2Mesh[1].enabled = false;
		_animator.Play(_eyeanimationHash[0], 1);
	}

	private int RandomCard(int StartPos)
	{
		if (SkillCard.ToArray().Length < 1)
		{
			SkillCard = new List<int>(DefaultSkillCard);
		}
		int num = SkillCard[OrangeBattleUtility.Random(0, SkillCard.ToArray().Length)];
		SkillCard.Remove(num);
		return num + StartPos;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		CanSummon = false;
		if (_eyestatus != 0)
		{
			ChangeModel(true, base.direction);
			ChangeEyeModel(true, base.direction);
		}
		for (int i = 0; i < PartCollide1.Count; i++)
		{
			if ((bool)PartCollide1[i])
			{
				PartCollide1[i].BackToPool();
			}
			if ((bool)PartCollide2[i])
			{
				PartCollide2[i].BackToPool();
			}
		}
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		if ((bool)_collideBullet2)
		{
			_collideBullet2.BackToPool();
		}
		if ((bool)_recoverBullet1)
		{
			_recoverBullet1.BackToPool();
		}
		if ((bool)_recoverBullet2)
		{
			_recoverBullet2.BackToPool();
		}
		StageUpdate.SlowStage();
		SetColliderEnable(false);
		SetStatus(MainStatus.Die);
	}

	private void ChangeModel(bool model, int dir)
	{
		for (int i = 0; i < Model1Mesh.Length; i++)
		{
			Model1Mesh[i].enabled = false;
			Model2Mesh[i].enabled = false;
		}
		Body1Mesh[0].enabled = false;
		Body2Mesh[0].enabled = false;
		Body1Mesh[1].enabled = false;
		Body2Mesh[1].enabled = false;
		if (model)
		{
			if (dir == 1)
			{
				for (int j = 0; j < Model2Mesh.Length; j++)
				{
					PartCollide2[j].HitCallback = null;
					PartCollide2[j].BackToPool();
				}
				EnemyCollide2.SetActive(true);
				base.AimTransform = EyeShootPos[1];
				_collideBullet2.Active(targetMask);
				Body2Mesh[1].enabled = true;
			}
			else
			{
				for (int k = 0; k < Model1Mesh.Length; k++)
				{
					PartCollide1[k].HitCallback = null;
					PartCollide1[k].BackToPool();
				}
				EnemyCollide1.SetActive(true);
				base.AimTransform = EyeShootPos[0];
				_collideBullet.Active(targetMask);
				Body1Mesh[1].enabled = true;
			}
			base.AllowAutoAim = true;
			return;
		}
		base.AllowAutoAim = false;
		if (dir == 1)
		{
			_recoverBullet2.Active(friendMask);
			_collideBullet2.BackToPool();
			for (int l = 0; l < Model2Mesh.Length; l++)
			{
				Model2Mesh[l].enabled = true;
				PartCollide2[l].HitCallback = Part2HitCallBack;
				PartCollide2[l].Active(targetMask);
			}
		}
		else
		{
			_recoverBullet1.Active(friendMask);
			_collideBullet.BackToPool();
			for (int m = 0; m < Model1Mesh.Length; m++)
			{
				Model1Mesh[m].enabled = true;
				PartCollide1[m].HitCallback = Part1HitCallBack;
				PartCollide1[m].Active(targetMask);
			}
		}
	}

	private void ChangeEyeModel(bool eye, int dir)
	{
		Body1Mesh[0].enabled = false;
		Body2Mesh[0].enabled = false;
		Body1Mesh[1].enabled = false;
		Body2Mesh[1].enabled = false;
		if (dir == 1)
		{
			if (eye)
			{
				Body2Mesh[0].enabled = true;
			}
			else
			{
				Body2Mesh[1].enabled = true;
			}
		}
		else if (eye)
		{
			Body1Mesh[0].enabled = true;
		}
		else
		{
			Body1Mesh[1].enabled = true;
		}
	}

	private void UpdateEyeCtrl()
	{
		_currentEyeFrame = _animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
		switch (_eyestatus)
		{
		case EyeStatus.Opening:
			if (_currentEyeFrame >= 1f)
			{
				_eyestatus = EyeStatus.Open;
			}
			break;
		case EyeStatus.Open:
			if (!_isEyeOpen)
			{
				_eyestatus = EyeStatus.Closing;
				PlaySE("BossSE03", "bs027_yellow08");
			}
			break;
		case EyeStatus.Closing:
			if (_currentEyeFrame >= 1f)
			{
				ChangeEyeModel(false, base.direction);
				_eyestatus = EyeStatus.Close;
			}
			break;
		case EyeStatus.Close:
			if (_isEyeOpen)
			{
				ChangeEyeModel(true, base.direction);
				_eyestatus = EyeStatus.Opening;
				PlaySE("BossSE03", "bs027_yellow07");
			}
			break;
		}
		_animator.Play(_eyeanimationHash[(int)_eyestatus], 1);
		_animator2.Play(_eyeanimationHash[(int)_eyestatus], 1);
	}

	private void Part1HitCallBack(object obj)
	{
		if (obj == null)
		{
			return;
		}
		Collider2D collider2D = obj as Collider2D;
		if (!(collider2D == null))
		{
			for (int i = 0; i < PartCollide1.Count; i++)
			{
				PartCollide1[i].GroupHitCheck(collider2D);
			}
		}
	}

	private void Part2HitCallBack(object obj)
	{
		if (obj == null || _mainStatus != MainStatus.Skill0)
		{
			return;
		}
		Collider2D collider2D = obj as Collider2D;
		if (!(collider2D == null))
		{
			for (int i = 0; i < PartCollide2.Count; i++)
			{
				PartCollide2[i].GroupHitCheck(collider2D);
			}
		}
	}

	private void HealCollideHitCallBack(object obj)
	{
		EnemyCollide1.SetActive(false);
		EnemyCollide2.SetActive(false);
		if (_recoverBullet1.IsActivate)
		{
			_recoverBullet1.BackToPool();
		}
		if (_recoverBullet2.IsActivate)
		{
			_recoverBullet2.BackToPool();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_002:
			CanSummon = true;
			DeadCallResult = false;
			base.DeadPlayCompleted = false;
			break;
		case AI_STATE.mob_003:
			CanSummon = false;
			DeadCallResult = false;
			base.DeadPlayCompleted = true;
			break;
		default:
			CanSummon = false;
			DeadCallResult = true;
			base.DeadPlayCompleted = true;
			break;
		}
	}
}
