#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS082_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill2 = 3,
		Skill3 = 4,
		Skill1 = 5,
		Die = 6
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
		ANI_DEBUT = 1,
		ANI_Skill0_START = 2,
		ANI_Skill0_LOOP = 3,
		ANI_Skill0_END = 4,
		ANI_Skill1_START = 5,
		ANI_Skill1_LOOP = 6,
		ANI_Skill1_END = 7,
		ANI_Skill2_START = 8,
		ANI_Skill2_LOOP = 9,
		ANI_Skill2_END = 10,
		ANI_Skill3_START1 = 11,
		ANI_Skill3_START2 = 12,
		ANI_Skill3_LOOP = 13,
		ANI_Skill3_END = 14,
		ANI_HURT = 15,
		ANI_DEAD = 16,
		MAX_ANIMATION_ID = 17
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int nDeadCount;

	private int[] _animationHash;

	private int[] SkillWeight1 = new int[2] { 5, 5 };

	private int[] SkillWeight2 = new int[3] { 5, 5, 5 };

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private bool HasShot;

	private Vector3 ShotPos;

	private Vector3 EndPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private Vector3 CenterPos;

	private float GroundYPos;

	private bool HasCheckRoom;

	[SerializeField]
	private int MoveSpd = 5000;

	private float MoveDis;

	private Vector3 StartPos;

	private Transform ModelOffest;

	private Vector3 RotateAngle;

	[Header("熱線")]
	private bool isLeft;

	[SerializeField]
	private ParticleSystem BeamFX;

	private CollideBullet BeamCollide;

	private bool bBeamSE;

	[Header("生怪")]
	[SerializeField]
	private Transform MousePos;

	[SerializeField]
	private int SummonEnemyLimit = 12;

	private int SpawnCount;

	private List<Vector3> SpawnPoints = new List<Vector3>();

	private int SpawnGroup;

	private static string[] EnemyGroup = new string[5] { "0,0", "0,1", "1,3", "0,2,3", "1,2,2" };

	private string ChoosePoints;

	[Header("衝撞")]
	[SerializeField]
	private int RamSpeed = 7500;

	[Header("包圍")]
	[SerializeField]
	private SkinnedMeshRenderer BodyMesh;

	[SerializeField]
	private int DefaultAttackTimes = 3;

	private int AttackTimes;

	[SerializeField]
	private float AppearTime = 2f;

	private float DissloveTime;

	private int DissloveFrame;

	private int AttackFrame;

	private bool bSkill3SE;

	[SerializeField]
	private float SurroundTime = 5f;

	private int SurroundFrame;

	private OrangeCharacter targetOC;

	private int TweenId = -1;

	[SerializeField]
	private float IdleWaitTime;

	private int IdleWaitFrame;

	private int AtkTimes;

	[SerializeField]
	public GameObject[] RenderModes;

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
			case "Skill2":
				NextSkill = MainStatus.Skill2;
				break;
			case "Skill3":
				NextSkill = MainStatus.Skill3;
				break;
			}
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[17];
		_animationHash[0] = Animator.StringToHash("BS082@idle");
		_animationHash[1] = Animator.StringToHash("BS082@debut");
		_animationHash[2] = Animator.StringToHash("BS082@skill_01_start");
		_animationHash[3] = Animator.StringToHash("BS082@skill_01_loop");
		_animationHash[4] = Animator.StringToHash("BS082@skill_01_end");
		_animationHash[5] = Animator.StringToHash("BS082@skill_02_start");
		_animationHash[6] = Animator.StringToHash("BS082@skill_02_loop");
		_animationHash[7] = Animator.StringToHash("BS082@skill_02_end");
		_animationHash[8] = Animator.StringToHash("BS082@skill_03_start");
		_animationHash[9] = Animator.StringToHash("BS082@skill_03_loop");
		_animationHash[10] = Animator.StringToHash("BS082@skill_03_end");
		_animationHash[11] = Animator.StringToHash("BS082@skill_04_disappear");
		_animationHash[12] = Animator.StringToHash("BS082@skill_04_appear");
		_animationHash[13] = Animator.StringToHash("BS082@skill_04_loop");
		_animationHash[14] = Animator.StringToHash("BS082@skill_04_end");
		_animationHash[15] = Animator.StringToHash("BS082@hurt_loop");
		_animationHash[16] = Animator.StringToHash("BS082@death");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelOffest = OrangeBattleUtility.FindChildRecursive(ref childs, "ModelOffest", true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (BeamFX == null)
		{
			BeamFX = OrangeBattleUtility.FindChildRecursive(ref childs, "p_bs082_beam", true).GetComponent<ParticleSystem>();
		}
		if (MousePos == null)
		{
			MousePos = OrangeBattleUtility.FindChildRecursive(ref childs, "mouth", true);
		}
		if (BodyMesh == null)
		{
			BodyMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS082", true).GetComponent<SkinnedMeshRenderer>();
		}
		if (BeamCollide == null)
		{
			BeamCollide = OrangeBattleUtility.FindChildRecursive(ref childs, "BeamCollide", true).gameObject.AddOrGetComponent<CollideBullet>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimTransform = ModelOffest;
		base.AimPoint = new Vector3(0f, 0f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
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
			if (nSet == 5)
			{
				if (netSyncData.nParam0 != -1)
				{
					SpawnGroup = netSyncData.nParam0;
				}
				if (netSyncData.sParam0 != null || netSyncData.sParam0 != string.Empty)
				{
					ChoosePoints = netSyncData.sParam0;
				}
			}
			UpdateDirection();
		}
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			if (_subStatus == SubStatus.Phase0)
			{
			}
			break;
		case MainStatus.Idle:
			if (!HasCheckRoom)
			{
				HasCheckRoom = CheckRoomSize();
			}
			UpdateDirection();
			_velocity.x = 0;
			ModelOffest.rotation = Quaternion.identity;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(20f * IdleWaitTime);
			_collideBullet.Active(targetMask);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_transform.position.x < CenterPos.x)
				{
					isLeft = true;
				}
				else
				{
					isLeft = false;
				}
				if (isLeft)
				{
					EndPos = new Vector2(MinPos.x, CenterPos.y);
				}
				else
				{
					EndPos = new Vector2(MaxPos.x, CenterPos.y);
				}
				StartPos = _transform.position;
				MoveDis = Vector2.Distance(StartPos, EndPos);
				_velocity = new VInt3((EndPos - StartPos).normalized) * MoveSpd * 0.001f;
				break;
			case SubStatus.Phase1:
				BeamFX.Play();
				PlaySE("BossSE04", "bs033_sigvrs01_lp");
				bBeamSE = true;
				base.SoundSource.AddLoopSE("BossSE04", "bs033_sigvrs02", 0.3f);
				BeamCollide.Active(targetMask);
				if (isLeft)
				{
					_velocity = VInt3.signRight * MoveSpd;
				}
				else
				{
					_velocity = VInt3.signLeft * MoveSpd;
				}
				break;
			case SubStatus.Phase2:
				BeamFX.Stop();
				base.SoundSource.RemoveLoopSE("BossSE04", "bs033_sigvrs02");
				PlaySE("BossSE04", "bs033_sigvrs01_stop");
				bBeamSE = false;
				BeamCollide.BackToPool();
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
				}
				UpdateDirection();
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
			{
				if (SpawnGroup < 0 || SpawnGroup > EnemyGroup.Length)
				{
					SpawnGroup = 0;
				}
				List<string> list = new List<string>(EnemyGroup[SpawnGroup].Split(','));
				List<string> list2 = new List<string>(ChoosePoints.Split(','));
				if (list.Count != list2.Count)
				{
					Debug.LogError("怪物數量與選擇的位置數量不同，檢查同步時是否少資料。 怪物群組" + SpawnGroup + " 包含生怪技能 " + EnemyGroup[SpawnGroup] + " 選擇的生怪點 " + ChoosePoints);
				}
				for (int i = 0; i < list2.Count; i++)
				{
					int num2 = 0;
					try
					{
						num2 = int.Parse(list2[i].ToString());
					}
					catch
					{
						num2 = 0;
						Debug.LogError("位置序列中，含有非數字的符號。");
					}
					int num3 = 0;
					try
					{
						num3 = int.Parse(list[i].ToString());
					}
					catch
					{
						num3 = 0;
						Debug.LogError("陣列設定的怪物ID，含有非數字的符號。");
					}
					EndPos = SpawnPoints[num2];
					BS082_SummonBullet obj3 = BulletBase.TryShotBullet(EnemyWeapons[7].BulletData, MousePos, EndPos - MousePos.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS082_SummonBullet;
					obj3.FreeDISTANCE = Vector3.Distance(MousePos.position, EndPos);
					obj3.SummonID = num3;
					obj3.BackCallback = SpawnEnemy;
				}
				break;
			}
			case SubStatus.Phase2:
				SpawnGroup = (SpawnGroup + 1) % EnemyGroup.Length;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				_collideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					EndPos = Target._transform.position;
				}
				else
				{
					EndPos = CenterPos;
				}
				TargetPos = new VInt3(EndPos);
				UpdateDirection();
				float num = Vector2.Angle((EndPos - _transform.position).normalized, Vector3.up * -base.direction);
				LeanTween.rotateZ(ModelOffest.gameObject, num - 90f, 0.5f);
				PlaySE("BossSE04", "bs033_sigvrs05");
				break;
			}
			case SubStatus.Phase1:
				StartPos = _transform.position;
				MoveDis = Vector2.Distance(EndPos, StartPos) - 0.5f;
				_velocity = new VInt3((EndPos - StartPos).normalized) * RamSpeed * 0.001f;
				if (MoveDis > 1f)
				{
					PlaySE("BossSE04", "bs033_sigvrs00");
				}
				break;
			case SubStatus.Phase2:
				ModelOffest.rotation = Quaternion.identity;
				_velocity = VInt3.zero;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				AttackTimes = DefaultAttackTimes;
				_collideBullet.BackToPool();
				break;
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				SetColliderEnable(false);
				DissloveFrame = GameLogicUpdateManager.GameFrame + (int)(AppearTime * 20f);
				TweenId = _characterMaterial.Disappear();
				_characterMaterial.ChangeDissolveTime(AppearTime);
				break;
			case SubStatus.Phase2:
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(new GameObject[1] { BodyMesh.gameObject }, ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer);
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					EndPos = Target._transform.position;
				}
				else
				{
					EndPos = CenterPos;
				}
				_transform.position = EndPos;
				Controller.LogicPosition = new VInt3(_transform.position);
				DissloveTime = _characterMaterial.GetDissloveTime();
				DissloveFrame = GameLogicUpdateManager.GameFrame + (int)(DissloveTime * 20f);
				AttackFrame = GameLogicUpdateManager.GameFrame + (int)(DissloveTime * 10f);
				HasShot = false;
				TweenId = _characterMaterial.Appear();
				break;
			case SubStatus.Phase3:
				if ((bool)targetOC)
				{
					EndPos = targetOC._transform.position + Vector3.up * (targetOC.Controller.Collider2D.size.y / 2f);
				}
				LeanTween.move(base.gameObject, EndPos, 0.3f).setOnUpdateVector3(delegate(Vector3 pos)
				{
					Controller.LogicPosition = new VInt3(pos);
				});
				SurroundFrame = GameLogicUpdateManager.GameFrame + (int)(SurroundTime * 20f);
				break;
			case SubStatus.Phase4:
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(new GameObject[1] { BodyMesh.gameObject }, ManagedSingleton<OrangeLayerManager>.Instance.RenderEnemy);
				ReleaseOC();
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				break;
			case SubStatus.Phase1:
			{
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_002)
				{
					StartCoroutine(BossDieFlow(_transform.position, "FX_BOSS_EXPLODE2", false, false));
					break;
				}
				return;
			}
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
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			break;
		case MainStatus.Idle:
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
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill0_LOOP;
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
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill2_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill2_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill2_END;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill3_START1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill3_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill3_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill3_END;
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_DEAD;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			return;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		if (mainStatus == MainStatus.Idle)
		{
			switch (_mainStatus)
			{
			case MainStatus.Debut:
				SetStatus(MainStatus.Idle);
				break;
			case MainStatus.Idle:
				if (AtkTimes == 0)
				{
					AtkTimes = 2;
					if (StageUpdate.runEnemys.Count <= SummonEnemyLimit)
					{
						mainStatus = MainStatus.Skill1;
						break;
					}
					if (!Target)
					{
						Target = _enemyAutoAimSystem.GetClosetPlayer();
					}
					mainStatus = ((!Target) ? MainStatus.Skill0 : ((MainStatus)((!(Target._transform.position.y > CenterPos.y + 1f)) ? WeightRandom(SkillWeight2, 2) : WeightRandom(SkillWeight1, 3))));
					AtkTimes--;
				}
				else
				{
					if (!Target)
					{
						Target = _enemyAutoAimSystem.GetClosetPlayer();
					}
					mainStatus = ((!Target) ? MainStatus.Skill0 : ((MainStatus)((!(Target._transform.position.y > CenterPos.y + 1f)) ? WeightRandom(SkillWeight2, 2) : WeightRandom(SkillWeight1, 3))));
					AtkTimes--;
				}
				break;
			}
		}
		if (DebugMode)
		{
			mainStatus = NextSkill;
		}
		if (mainStatus == MainStatus.Skill1)
		{
			List<Vector3> spawnpos = new List<Vector3>(SpawnPoints.ToArray());
			ChoosePoints = "";
			int num = EnemyGroup[SpawnGroup].Split(',').Length;
			for (int i = 0; i < num; i++)
			{
				if (i > 0)
				{
					ChoosePoints += ",";
				}
				int pos = OrangeBattleUtility.Random(0, spawnpos.Count);
				ChoosePoints += SpawnPoints.FindIndex((Vector3 point) => point == spawnpos[pos]);
				spawnpos.Remove(spawnpos[pos]);
			}
		}
		if (mainStatus != 0 && CheckHost())
		{
			if (mainStatus == MainStatus.Skill1)
			{
				UploadEnemyStatus((int)mainStatus, false, new object[1] { SpawnGroup }, new object[1] { ChoosePoints });
			}
			else
			{
				UploadEnemyStatus((int)mainStatus);
			}
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
				base.AllowAutoAim = false;
				SetStatus(MainStatus.Debut, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.05f && !BodyMesh.enabled && (bool)BodyMesh)
				{
					BodyMesh.enabled = true;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_introReady && IntroCallBack != null)
				{
					IntroCallBack();
					base.AllowAutoAim = true;
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus && GameLogicUpdateManager.GameFrame > IdleWaitFrame)
			{
				UpdateRandomState();
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Vector2.Distance(_transform.position, StartPos) > MoveDis)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((isLeft && _transform.position.x > MaxPos.x) || (!isLeft && _transform.position.x < MinPos.x))
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
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
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
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
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Vector2.Distance(StartPos, _transform.position) > MoveDis || _transform.position.y <= GroundYPos - 0.5f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
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
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > DissloveFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!HasShot && GameLogicUpdateManager.GameFrame > AttackFrame)
				{
					HasShot = true;
					if (_collideBullet != null)
					{
						_collideBullet.HitCallback = CatchPlayer;
						_collideBullet.Active(targetMask);
					}
				}
				if (GameLogicUpdateManager.GameFrame > DissloveFrame)
				{
					SetColliderEnable(true);
					_collideBullet.HitCallback = null;
					_collideBullet.BackToPool();
					if (targetOC != null)
					{
						PlaySE("BossSE04", "bs033_sigvrs04_lp");
						bSkill3SE = true;
						SetStatus(MainStatus.Skill3, SubStatus.Phase3);
					}
					else if (--AttackTimes > 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame > SurroundFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
					PlaySE("BossSE04", "bs033_sigvrs04_stop");
					bSkill3SE = false;
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0 && _currentFrame > 0f)
			{
				if (nDeadCount > 2)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				else
				{
					nDeadCount++;
				}
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.GAME_PAUSE, PauseTween);
			IgnoreGravity = true;
			HasCheckRoom = CheckRoomSize();
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			BeamCollide.UpdateBulletData(EnemyWeapons[1].BulletData);
			BeamCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			if ((bool)BodyMesh)
			{
				BodyMesh.enabled = false;
			}
			SpawnGroup = 0;
			SetStatus(MainStatus.Debut);
		}
		else
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.GAME_PAUSE, PauseTween);
			if (bBeamSE)
			{
				base.SoundSource.RemoveLoopSE("BossSE", "bs033_sigvrs02");
				PlaySE("BossSE04", "bs033_sigvrs01_stop");
				bBeamSE = false;
			}
			SpawnPoints.Clear();
			_collideBullet.BackToPool();
		}
	}

	private bool CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y + 2f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.up, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D4 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.down, 30f, layerMask, _transform);
		if (!raycastHit2D3)
		{
			Debug.LogError("沒有偵測到天花板或地板，之後一些技能無法準確判斷位置");
			return false;
		}
		MaxPos = new Vector3(raycastHit2D2.point.x - Controller.Collider2D.size.x, raycastHit2D3.point.y - Controller.Collider2D.size.y, 0f);
		MinPos = new Vector3(raycastHit2D.point.x + Controller.Collider2D.size.x, raycastHit2D4.point.y + Controller.Collider2D.size.y, 0f);
		GroundYPos = raycastHit2D4.point.y;
		CenterPos = (MaxPos + MinPos) / 2f;
		return true;
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
		ModelTransform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * (float)base.direction);
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

	private int WeightRandom(int[] WeightArray, int SkillStart)
	{
		int num = 0;
		int num2 = 0;
		int num3 = WeightArray.Length;
		for (int i = 0; i < num3; i++)
		{
			num2 += WeightArray[i];
		}
		int num4 = OrangeBattleUtility.Random(0, num2);
		for (int j = 0; j < num3; j++)
		{
			num += WeightArray[j];
			if (num4 < num)
			{
				return j + SkillStart;
			}
		}
		return 0;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if ((bool)targetOC)
		{
			ReleaseOC();
		}
		if (_mainStatus != MainStatus.Die)
		{
			ModelOffest.rotation = Quaternion.identity;
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if (bSkill3SE)
			{
				PlaySE("BossSE04", "bs033_sigvrs04_stop");
				bSkill3SE = false;
			}
			if (bBeamSE)
			{
				base.SoundSource.RemoveLoopSE("BossSE", "bs033_sigvrs02");
				PlaySE("BossSE04", "bs033_sigvrs01_stop");
				bBeamSE = false;
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	private void CatchPlayer(object obj)
	{
		if (obj == null || (bool)targetOC)
		{
			return;
		}
		Collider2D collider2D = obj as Collider2D;
		if (!collider2D)
		{
			return;
		}
		targetOC = OrangeBattleUtility.GetHitTargetOrangeCharacter(collider2D);
		if (!targetOC || targetOC.IsStun)
		{
			return;
		}
		if ((bool)targetOC.IsUnBreakX())
		{
			targetOC = null;
			return;
		}
		targetOC.SetStun(true);
		if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right * base.direction, 2f, Controller.collisionMask, _transform))
		{
			UpdateDirection(-base.direction);
		}
		_collideBullet.HitCallback = null;
		_collideBullet.BackToPool();
	}

	private void ReleaseOC()
	{
		if ((bool)targetOC)
		{
			targetOC.SetStun(false);
		}
		targetOC = null;
	}

	private void SpawnEnemy(object obj)
	{
		if ((int)Hp <= 0 || !Activate || _mainStatus == MainStatus.Die)
		{
			return;
		}
		BS082_SummonBullet bS082_SummonBullet = null;
		if (obj != null)
		{
			bS082_SummonBullet = obj as BS082_SummonBullet;
		}
		if (bS082_SummonBullet == null)
		{
			Debug.LogError("子彈資料有誤。");
			return;
		}
		int summonID = bS082_SummonBullet.SummonID;
		Vector3 position = bS082_SummonBullet._transform.position;
		MOB_TABLE enemy = GetEnemy((int)EnemyWeapons[summonID + 3].BulletData.f_EFFECT_X);
		if (enemy == null)
		{
			Debug.LogError("要生成的怪物資料有誤，生怪技能ID " + summonID + " 怪物GroupID " + EnemyWeapons[summonID + 3].BulletData.f_EFFECT_X);
			return;
		}
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(enemy, sNetSerialID + SpawnCount);
		SpawnCount++;
		if ((bool)enemyControllerBase)
		{
			if (summonID == 3)
			{
				enemyControllerBase.SetPositionAndRotation(position, position.x > CenterPos.x);
			}
			else
			{
				enemyControllerBase.SetPositionAndRotation(position, base.direction == -1);
			}
			enemyControllerBase.SetActive(true);
		}
	}

	private MOB_TABLE GetEnemy(int nGroupID)
	{
		MOB_TABLE[] mobArrayFromGroup = ManagedSingleton<OrangeTableHelper>.Instance.GetMobArrayFromGroup(nGroupID);
		for (int i = 0; i < mobArrayFromGroup.Length; i++)
		{
			if (mobArrayFromGroup[i].n_DIFFICULTY == StageUpdate.gDifficulty)
			{
				return mobArrayFromGroup[i];
			}
		}
		return null;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_001:
		{
			Vector3[] collection = new Vector3[4]
			{
				new Vector3(86.9f, -36.2f, 0f),
				new Vector3(72f, -36.2f, 0f),
				new Vector3(86.9f, -39.2f, 0f),
				new Vector3(72f, -39.2f, 0f)
			};
			SpawnPoints = new List<Vector3>(collection);
			break;
		}
		case AI_STATE.mob_002:
		{
			Vector3[] collection = new Vector3[4]
			{
				new Vector3(883.77f, 242.9f, 0f),
				new Vector3(868f, 242.9f, 0f),
				new Vector3(883.77f, 239.87f, 0f),
				new Vector3(868f, 239.87f, 0f)
			};
			SpawnPoints = new List<Vector3>(collection);
			break;
		}
		}
	}

	public void PauseTween(bool isPause)
	{
		if (TweenId != -1)
		{
			if (isPause)
			{
				LeanTween.pause(TweenId);
			}
			else
			{
				LeanTween.resume(TweenId);
			}
		}
	}
}
